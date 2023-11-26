using System;
using UnityEngine;

namespace GridModule
{
    public class GridCell : MonoBehaviour
    {
        public bool IsEmpty { get; private set; } = true;

        public void Toggle(bool toggle)
        {
            IsEmpty = toggle;
        }
    }
}