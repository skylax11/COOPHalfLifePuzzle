using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Indicator : MonoBehaviour
{
    public Image energyBar;
    void Start()
    {
        TauGun.OnHoldStarted += FillBar;
        TauGun.OnHoldUnLeashed += DrainBar;
    }
    private void FillBar()
    {
        energyBar.fillAmount += (Time.deltaTime * 8f) / 20;
    }
    private void DrainBar()
    {
        energyBar.fillAmount -= (Time.deltaTime * 16f) / 20;
    }
}
