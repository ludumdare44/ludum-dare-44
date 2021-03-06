﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentRule : MonoBehaviour
{
    public GameModifiers gameModifiers;
    public OpponentController opponent;
    public GameObject player;

    private bool _maxInPlay;
    private Vector3 currentVelocity;

    void Start()
    {
        _maxInPlay = false;
        opponent.RegisterListener(this);
    }

    void Update()
    {
        if (!_maxInPlay && gameModifiers.IsMaxDangerLevel())
        {
            _maxInPlay = true;
            opponent.gameObject.SetActive(true);
            opponent.transform.position = CopyPlayerMinus(20);
            currentVelocity = new Vector3();
        } else if (_maxInPlay && !gameModifiers.IsMaxDangerLevel())
        {
            _maxInPlay = false;
            opponent.gameObject.SetActive(false);
        }
    }

    void FixedUpdate()
    {
        if (_maxInPlay)
        {
            opponent.transform.position = (Vector3.SmoothDamp(
                opponent.transform.position,
                CopyPlayerMinus(0),
                ref currentVelocity,
                0.3F
            ));
        }
    }

    private Vector3 CopyPlayerMinus(float minus)
    {
        return new Vector3(
            player.transform.position.x - minus,
            opponent.transform.position.y,
            opponent.transform.position.z
        );
    }

    public void DoActionOnCollision(Collider opponent, Collider other)
    {
        if (!other.gameObject.CompareTag("prop_obstacle"))
        {
            return;
        }
        
        var obstacleProp = other.transform.GetComponent<ObstacleProp>();
        if (obstacleProp != null && !obstacleProp.IsAlreadyCollided())
        {
            obstacleProp.DoCollide(opponent, false);
        }
    }
}
