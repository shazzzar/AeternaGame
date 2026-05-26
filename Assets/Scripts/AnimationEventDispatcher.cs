using UnityEngine;

public class AnimationEventDispatcher : MonoBehaviour
{
    private MiningSystem miningSystem;

    void Awake()
    {
        // Find MiningSystem on this object or any parent (usually on the prefab root)
        miningSystem = GetComponentInParent<MiningSystem>();
        if (miningSystem == null)
        {
            Debug.LogWarning($"AnimationEventDispatcher on {gameObject.name} could not find MiningSystem in parents!");
        }
    }

    // This method is called by the Animation Event
    public void PlayMiningSwingSound()
    {
        if (miningSystem != null)
        {
            miningSystem.PlayMiningSwingSound();
        }
        else
        {
            // Fallback: try to find it if it wasn't there at Awake
            miningSystem = GetComponentInParent<MiningSystem>();
            if (miningSystem != null) 
            {
                miningSystem.PlayMiningSwingSound();
            }
            else
            {
                Debug.LogError($"AnimationEventDispatcher on {gameObject.name} failed to find MiningSystem!");
            }
        }
    }
}
