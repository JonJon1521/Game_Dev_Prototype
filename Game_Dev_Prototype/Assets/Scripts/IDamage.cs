using UnityEngine;

public interface IDamage
{
  void takeDamage(int amount);

    // Shack for 0.15 seconds with the magnitud of 0.2

    GetComponentInChildren<cameraController>().shack(0.15f, 0.2f);
}
