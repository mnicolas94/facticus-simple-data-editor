using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SimpleDataEditor.Editor.VisualElements
{
    /// <summary>
    /// source https://discussions.unity.com/t/how-to-register-drag-and-click-events-on-the-same-visualelement/860121/2
    /// </summary>
    public class DragAndDropManipulator : MouseManipulator
    {
        readonly EventCallback<PointerDownEvent> _pointerDownHandler;
        readonly EventCallback<PointerMoveEvent> _onPointerMove;
        readonly EventCallback<PointerUpEvent> _pointerUpHandler;

        enum DragState
        {
            AtRest,
            Ready,
            Dragging
        }
        private DragState _dragState;
        
        private readonly Object[] _objectReferences = new Object[1];

        public DragAndDropManipulator()
        {
            _pointerDownHandler = OnPointerDown;
            _onPointerMove = OnPointerMove;
            _pointerUpHandler = OnPointerUp;
            _dragState = DragState.AtRest;
        }
        
        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button == 0)
            {
                _dragState = DragState.Ready;
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (_dragState == DragState.Ready && evt.button == 0)
            {
                _dragState = DragState.AtRest;
            }
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (_dragState == DragState.Ready)
            {
                _dragState = DragState.Dragging;
                
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.StartDrag($"Dragging {target}");
                _objectReferences[0] = target.userData as Object;
                DragAndDrop.objectReferences = _objectReferences;
            }
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback(_pointerDownHandler);
            target.RegisterCallback(_onPointerMove);
            target.RegisterCallback(_pointerUpHandler);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback(_pointerDownHandler);
            target.UnregisterCallback(_onPointerMove);
            target.UnregisterCallback(_pointerUpHandler);
        }
    }
}