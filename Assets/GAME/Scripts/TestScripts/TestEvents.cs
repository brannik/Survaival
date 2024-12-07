using System;
using UnityEngine;

public class TestEvents : MonoBehaviour
{
    public event EventHandler<CustomEventArgs> MyCustomEvent; // event
    public class CustomEventArgs : EventArgs{ // arguments of any type
        public int value;
    }

    private void CallSimpleEvent(){
        MyCustomEvent?.Invoke(this,new CustomEventArgs { value = 10}); // trigger the event
    }

    void Awake(){
        this.MyCustomEvent += TestFunction; // listen for the event
    }

    private void TestFunction(object sender, CustomEventArgs e) // callback
    {
        throw new NotImplementedException();
    }
}
