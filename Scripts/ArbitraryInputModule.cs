// Decompiled with JetBrains decompiler
// Type: UnityEngine.EventSystems.StandaloneInputModule
// Assembly: UnityEngine.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2216A18B-AF52-44A5-85A0-A1CAA19C1090
// Assembly location: C:\Users\Blake\sandbox\unity\test-project\Library\UnityAssemblies\UnityEngine.UI.dll

using System;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UnityEngine.EventSystems
{
    ///based on https://github.com/VREALITY/ViveUGUIModule
    
    public class ArbitraryInputModule : BaseInputModule
    {
        public static ArbitraryInputModule instance;

        public float NormalCursorScale = 0.00025f;

        [Tooltip("Indicates whether or not the gui was hit by any controller this frame")]
        public bool GuiHit;
        public Camera ControllerCamera { get; private set; }

        protected override void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            if (instance != this)
            {
                Debug.LogError("Why are there two instances of a singleton?");
                return;
            }

            ControllerCamera = new GameObject("Controller UI Camera").AddComponent<Camera>();
            ControllerCamera.clearFlags = CameraClearFlags.Nothing; //CameraClearFlags.Depth;
            ControllerCamera.cullingMask = 0; // 1 << LayerMask.NameToLayer("UI"); 
            ControllerCamera.nearClipPlane = 0.001f;
            ControllerCamera.stereoTargetEye = StereoTargetEyeMask.None;

            Canvas[] canvases = GameObject.FindObjectsOfType<Canvas>();

            foreach (Canvas canvas in canvases)
            {
                canvas.worldCamera = ControllerCamera;
            }
        }

        protected override void Start()
        {
            base.Start();
        }

        public override void Process()
        {
            GuiHit = false;
            //ButtonUsed = false;

            // send update events if there is a selected object - this is important for InputField to receive keyboard events
            SendUpdateEventToSelectedObject();

            foreach (ArbitraryInput pointer in ArbitraryInput.instances)
            {
                //Check if the Pointer is Active in the heirarchy. If it isn't, turn off the cursor and continue
                if (pointer.gameObject.activeInHierarchy == false)
                {
                    pointer.isHittingUI = false;

                    continue;
                }

                UpdateCameraPosition(pointer);

                bool hit = GetLookPointerEventData(pointer);
                if (hit == false)
                    continue;

                pointer.CurrentPoint = pointer.PointEvent.pointerCurrentRaycast.gameObject;

                // handle enter and exit events (highlight)
                base.HandlePointerExitAndEnter(pointer.PointEvent, pointer.CurrentPoint);

                UpdateCursor(pointer, pointer.PointEvent);

                if (pointer != null)
                {
                    if (pointer.buttonDown)
                    {
                        ClearSelection();

                        pointer.PointEvent.pressPosition = pointer.PointEvent.position;
                        pointer.PointEvent.pointerPressRaycast = pointer.PointEvent.pointerCurrentRaycast;
                        pointer.PointEvent.pointerPress = null;

                        if (pointer.CurrentPoint != null)
                        {
                            pointer.CurrentPressed = pointer.CurrentPoint;

                            GameObject newPressed = ExecuteEvents.ExecuteHierarchy(pointer.CurrentPressed.gameObject, pointer.PointEvent, ExecuteEvents.pointerDownHandler);

                            if (newPressed == null)
                            {
                                // some UI elements might only have click handler and not pointer down handler
                                newPressed = ExecuteEvents.ExecuteHierarchy(pointer.CurrentPressed, pointer.PointEvent, ExecuteEvents.pointerClickHandler);
                                if (newPressed != null)
                                {
                                    pointer.CurrentPressed = newPressed;
                                }
                            }
                            else
                            {
                                pointer.CurrentPressed = newPressed;
                                // we want to do click on button down at same time, unlike regular mouse processing
                                // which does click when mouse goes up over same object it went down on
                                // reason to do this is head tracking might be jittery and this makes it easier to click buttons
                                ExecuteEvents.Execute(newPressed, pointer.PointEvent, ExecuteEvents.pointerClickHandler);
                            }

                            if (newPressed != null)
                            {
                                pointer.PointEvent.pointerPress = newPressed;
                                pointer.CurrentPressed = newPressed;
                                Select(pointer.CurrentPressed);
                                //ButtonUsed = true;
                            }

                            ExecuteEvents.Execute(pointer.CurrentPressed, pointer.PointEvent, ExecuteEvents.beginDragHandler);
                            pointer.PointEvent.pointerDrag = pointer.CurrentPressed;
                            pointer.CurrentDragging = pointer.CurrentPressed;
                        }
                    }

                    if (pointer.buttonUp)
                    {
                        if (pointer.CurrentDragging)
                        {
                            ExecuteEvents.Execute(pointer.CurrentDragging, pointer.PointEvent, ExecuteEvents.endDragHandler);

                            if (pointer.CurrentPoint != null)
                            {
                                ExecuteEvents.ExecuteHierarchy(pointer.CurrentPoint, pointer.PointEvent, ExecuteEvents.dropHandler);
                            }
                            pointer.PointEvent.pointerDrag = null;
                            pointer.CurrentDragging = null;
                        }
                        if (pointer.CurrentPressed)
                        {
                            ExecuteEvents.Execute(pointer.CurrentPressed, pointer.PointEvent, ExecuteEvents.pointerUpHandler);
                            pointer.PointEvent.rawPointerPress = null;
                            pointer.PointEvent.pointerPress = null;
                            pointer.CurrentPressed = null;
                        }
                    }

                    // drag handling
                    if (pointer.CurrentDragging != null)
                    {
                        ExecuteEvents.Execute(pointer.CurrentDragging, pointer.PointEvent, ExecuteEvents.dragHandler);
                    }
                }
            }
        }

        // use screen midpoint as locked pointer location, enabling look location to be the "mouse"
        private bool GetLookPointerEventData(ArbitraryInput pointer)
        {
            if (pointer.PointEvent == null)
                pointer.PointEvent = new PointerEventData(base.eventSystem);
            else
                pointer.PointEvent.Reset();

            pointer.PointEvent.delta = Vector2.zero;
            pointer.PointEvent.position = new Vector2(Screen.width / 2, Screen.height / 2);
            pointer.PointEvent.scrollDelta = Vector2.zero;

            base.eventSystem.RaycastAll(pointer.PointEvent, m_RaycastResultCache);
            pointer.PointEvent.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
            if (pointer.PointEvent.pointerCurrentRaycast.gameObject != null)
            {
                GuiHit = true; //gets set to false at the beginning of the process event
            }

            m_RaycastResultCache.Clear();

            return true;
        }

        // update the cursor location and whether it is enabled
        // this code is based on Unity's DragMe.cs code provided in the UI drag and drop example
        private void UpdateCursor(ArbitraryInput pointer, PointerEventData pointData)
        {
            if (pointer.PointEvent.pointerCurrentRaycast.gameObject != null)
            {
                pointer.isHittingUI = true;

                if (pointData.pointerEnter != null)
                {
                    RectTransform draggingPlane = pointData.pointerEnter.GetComponent<RectTransform>();
                    Vector3 globalLookPos;
                    if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane, pointData.position, pointData.enterEventCamera, out globalLookPos))
                    {
                        pointer.CursorPosition = globalLookPos;
                        pointer.CursorRotation = draggingPlane.rotation;

                        float lookPointDistance = (pointer.CursorPosition - Camera.main.transform.position).magnitude;
                        float cursorScale = lookPointDistance * NormalCursorScale;
                        if (cursorScale < NormalCursorScale)
                        {
                            cursorScale = NormalCursorScale;
                        }

                        pointer.CursorScale = Vector3.one * cursorScale;
                    }
                }
            }
            else
            {
                pointer.isHittingUI = false;
            }
        }

        // clear the current selection
        public void ClearSelection()
        {
            if (base.eventSystem.currentSelectedGameObject)
            {
                base.eventSystem.SetSelectedGameObject(null);
            }
        }

        // select a game object
        private void Select(GameObject go)
        {
            ClearSelection();

            if (ExecuteEvents.GetEventHandler<ISelectHandler>(go))
            {
                base.eventSystem.SetSelectedGameObject(go);
            }
        }

        // send update event to selected object
        // needed for InputField to receive keyboard input
        private bool SendUpdateEventToSelectedObject()
        {
            if (base.eventSystem.currentSelectedGameObject == null)
                return false;

            BaseEventData data = GetBaseEventData();

            ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);

            return data.used;
        }

        private void UpdateCameraPosition(ArbitraryInput pointer)
        {
            ControllerCamera.transform.position = pointer.transform.position;
            ControllerCamera.transform.forward = pointer.transform.forward;
        }
    }

}