using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//This is currently not being used but I didn't want to delete it just yet.

public class StateMachine : MonoBehaviour
{

    protected State currentState;

    public void SetState(State newState)
    {
        currentState = newState;
    }


    public abstract class State
    {
        public virtual IEnumerator Standing()
        {
            yield break;
        }

        public virtual IEnumerator Walking()
        {
            yield break;
        }

        public virtual IEnumerator Jumping()
        {
            yield break;
        }

        public virtual IEnumerator Dashing()
        {
            yield break;
        }

        public virtual IEnumerator Dodging()
        {
            yield break;
        }

        public virtual IEnumerator Attacking()
        {
            yield break;
        }
    }


}
