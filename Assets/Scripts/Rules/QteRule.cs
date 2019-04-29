﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class QteRule : MonoBehaviour
{
    private QteAction _promptFor;
    private QteAction? _actionThisTick;

    public Text qteButtonUI;
    public Text qteTimeRemainingUI;
    public GameModifiers gameModifiers;
    
    private float _timeRemainingForCurrentPrompt;
    private float _qteDuration;
    
    private int _qteMandatoryCount;
    private int _qtePerformed;

    public enum QteAction
    {
        UP, DOWN, LEFT, RIGHT
    }
    
    void Start()
    {
    }

    void Update()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (_actionThisTick != null)
        {
            if (_actionThisTick == _promptFor)
            {
                PromptSuccessful();
            }
            else
            {
                AudioManager.instance.FailRobbing();
                Failure(GameModifiers.DangerSource.QTE_EXCEEDED_TIME_LIMIT);
            }

            _actionThisTick = null;
        }
        else
        {
            _timeRemainingForCurrentPrompt -= Time.deltaTime;
            if (_timeRemainingForCurrentPrompt > 0)
            {
                qteTimeRemainingUI.text = new String('.', 1 + (int)Math.Floor(_timeRemainingForCurrentPrompt * 50));
            }
            else
            {
                AudioManager.instance.FailRobbing();
                Failure(GameModifiers.DangerSource.QTE_EXCEEDED_TIME_LIMIT);
            }
        }
    }

    private void PromptSuccessful()
    {
        _qtePerformed++;
        GeneratePrompt();
    }

    private void Failure(GameModifiers.DangerSource dangerSource)
    {
        if (IsQteConsideredHarmful()) {
            gameModifiers.TakeDamage(dangerSource);
            
        }
        else
        {
            var bonusQteCount = _qtePerformed - _qteMandatoryCount;
            gameModifiers.SuccessfulQte(bonusQteCount);
        }
        End();
    }

    private bool IsQteConsideredHarmful()
    {
        return _qtePerformed < _qteMandatoryCount;
    }

    public void Input(QteAction action)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        _actionThisTick = action;
    }

    private void End()
    {
        enabled = false;
        HidePrompt();
    }

    public void Begin()
    {
        if (isActiveAndEnabled)
        {
            return;
        }

        enabled = true;
        _qtePerformed = 0;
        if (gameModifiers.IsMaxDangerLevel())
        {
            _qteMandatoryCount = 10;
            _qteDuration = 1F;
        }
        else
        {
            _qteMandatoryCount = 2 + Random.Range(0, 5);
            _qteDuration = 1F;
        }
        
        gameModifiers.ForceDisableInvulnerability();
        GeneratePrompt();
    }

    private void GeneratePrompt()
    {
        _promptFor = NextPrompt();
        if (IsQteConsideredHarmful()) {
            _timeRemainingForCurrentPrompt = _qteDuration;
        }
        else
        {
            _timeRemainingForCurrentPrompt = _qteDuration * CalculateBonusTimeFactor();
        }
        ShowPrompt();
    }

    private float CalculateBonusTimeFactor()
    {
        var linear = 1 + _qtePerformed - _qteMandatoryCount;
        if (linear < 10)
        {
            return 1F - linear * 0.1F;
        }
        else
        {
            return 0.1F / (2F * (linear - 10F));
        }
    }

    private void ShowPrompt()
    {
        switch (_promptFor)
        {
            case QteAction.UP:
                qteButtonUI.text = "↑";
                break;
            case QteAction.DOWN:
                qteButtonUI.text = "↓";
                break;
            case QteAction.LEFT:
                qteButtonUI.text = "←";
                break;
            case QteAction.RIGHT:
                qteButtonUI.text = "→";
                break;
        }
    }

    private void HidePrompt()
    {
        qteButtonUI.text = "";
        qteTimeRemainingUI.text = "";
    }

    private static QteAction NextPrompt()
    {
        var actions = Enum.GetValues(typeof(QteAction));
        return (QteAction)actions.GetValue((int) Math.Floor((double)Random.Range(0, actions.Length)));
    }

    public bool IsQteRunning() {
        return isActiveAndEnabled;
    }

    private void ShowBountyNotification()
    {
        var bountyup = Random.Range(1000, 10000);
        GameObject.Find("UIUpdater").GetComponent<UIUpdater>().UpBounty(bountyup);
    }
}
