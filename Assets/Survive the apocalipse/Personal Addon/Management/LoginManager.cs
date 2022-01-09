using EasyMobile;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginManager : MonoBehaviour
{
    public static LoginManager singleton;
    bool authenticated = false;
    bool isServiceAvailable = false;

    public void Awake()
    {
        if (!RuntimeManager.IsInitialized())
            RuntimeManager.Init();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!singleton) singleton = this;

        if (!GameServices.IsInitialized())
        {
            GameServices.ManagedInit();
        }
    }

    void OnEnable() { GameServices.UserLoginSucceeded += OnUserLoginSucceeded; GameServices.UserLoginFailed += OnUserLoginFailed; }

    private void OnUserLoginSucceeded()
    {
        Debug.Log("Player is logged in");
        UILogin.singleton.accountInput.gameObject.SetActive(true);
        UILogin.singleton.accountInput.text = GameServices.LocalUser.userName;
        UILogin.singleton.auth.loginAccount = GameServices.LocalUser.userName;
    }

    void OnDisable() { GameServices.UserLoginSucceeded -= OnUserLoginSucceeded; GameServices.UserLoginFailed -= OnUserLoginFailed; }

    void OnUserLoginFailed() { Debug.Log("User	login	failed."); }


    // Update is called once per frame
    void Update()
    {
    }
}
