using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    //[NonSerialized]
    public Vector2 Position;
    //[NonSerialized]
    public Vector2 MoveToPosition;

    [NonSerialized]
    public Transform _transform;

    [NonSerialized]
    public GameObject _gameObject;
    [NonSerialized]
    public SpriteRenderer _spriteRenderer;

    private float moveSpeed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
