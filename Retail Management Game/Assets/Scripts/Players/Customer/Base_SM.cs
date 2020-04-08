using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Base_SM
{
    void StartState();

    void ExitState();

    void UpdateState();

    void FixedUpdateState();
}
