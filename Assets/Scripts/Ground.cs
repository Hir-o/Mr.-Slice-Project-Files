using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;

public class Ground : MonoBehaviour
{
    private Rigidbody2D    _rigidbody2D;
    private SpriteRenderer _spriteRenderer, _shadowSpriteRenderer;
    private GameObject     _shadow,         _shadowGobj;
    private Color          _shadowColor;
    private Slicer2DSound _slicerSound;

    public bool OverrideMass;

    [ShowIf("OverrideMass")]
    public float NewMass = 5f;

    private void Start()
    {
        _rigidbody2D    = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _shadow         = new GameObject();

        gameObject.layer = LayerMask.NameToLayer("Obstacle");

        if (OverrideMass)
            if (_rigidbody2D != null)
                _rigidbody2D.mass = NewMass;

        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = ColorController.Instance.GroundColor;

            InstatiateShadow();
        }

        if (GetComponent<Slicer2D>())
        {
            _slicerSound      = gameObject.AddComponent<Slicer2DSound>();
            _slicerSound.clip = Resources.Load<AudioClip>("Slice2");
        }
    }

    private void InstatiateShadow()
    {
        _shadowGobj = Instantiate(_shadow, transform.position, transform.rotation);

        _shadowGobj.transform.localScale = transform.localScale;

        _shadowGobj.AddComponent<SpriteRenderer>();

        _shadowSpriteRenderer = _shadowGobj.GetComponent<SpriteRenderer>();

        _shadowSpriteRenderer.sortingLayerName = _spriteRenderer.sortingLayerName;
        _shadowSpriteRenderer.sortingOrder     = _spriteRenderer.sortingOrder - 1;

        _shadowColor   = _spriteRenderer.color;
        _shadowColor.a = .4f;

        _shadowSpriteRenderer.sprite = _spriteRenderer.sprite;
        _shadowSpriteRenderer.color  = _shadowColor;
    }

    private void OnMouseOver()
    {
        if (EventSystem.current.IsPointerOverGameObject()) { return; }
        
        Player.Instance?.LineDrawer.SetColorToRed();
    }

    private void OnMouseExit()
    {
        if (EventSystem.current.IsPointerOverGameObject()) { return; }
        
        if (LevelRules.Instance.AllowEnergy && LevelRules.Instance.Energy <= 0) return;

        Player.Instance?.LineDrawer.ResetColor();
    }

    private void OnBecameInvisible()
    {
        gameObject.SetActive(false);
    }
}