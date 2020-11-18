using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class LineDrawer : MonoBehaviour
{
    [SerializeField] private Color _colorWhite, _colorRed;

    [SerializeField] private float _lineRendererMaxLength = 8f;

    private MovementEventSubscriber _movementEventSubscriber;
    private LineRenderer            _lineRenderer;
    private Vector2                 _mousePosition;

    private bool _isMouseOverUI;

    private void Start()
    {
        _lineRenderer            = GetComponent<LineRenderer>();
        _movementEventSubscriber = GetComponentInParent<MovementEventSubscriber>();
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            _isMouseOverUI = true;
            Player.Instance?.LineDrawer.SetColorToRed();
        }
        else
        {
            if (_isMouseOverUI)
            {
                _isMouseOverUI = false;
                Player.Instance?.LineDrawer.ResetColor();
            }
        }
        
        _mousePosition = ObjectHolder.Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition);

        _lineRenderer.SetPosition(0, Player.Instance.transform.position);

        if (_movementEventSubscriber.DashDestinations.Count > 0)
        {
            _lineRenderer.positionCount = _movementEventSubscriber.DashDestinations.Count + 2;

            for (int i = 1; i < _lineRenderer.positionCount - 1; i++)
                _lineRenderer.SetPosition(i, _movementEventSubscriber.DashDestinations[i - 1]);

            _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, _mousePosition);
        }
        else
        {
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPosition(1, _mousePosition);
        }
    }

    public void SetPosition(List<Vector2> dashDestinations, Vector2 mouseClickPosition)
    {
        if (dashDestinations.Count > 0)
            _lineRenderer.SetPosition(0, dashDestinations.Last());
        else
            _lineRenderer.SetPosition(0, Player.Instance.transform.position);

        _lineRenderer.SetPosition(1, mouseClickPosition);
    }

    public void SetColorToRed()
    {
        _lineRenderer.DOColor(new Color2(_colorWhite, _colorWhite), new Color2(_colorRed, _colorRed), 0f);
    }

    public void ResetColor()
    {
        _lineRenderer.DOColor(new Color2(_colorWhite, _colorWhite), new Color2(_colorWhite, _colorWhite), 0f);
    }
}