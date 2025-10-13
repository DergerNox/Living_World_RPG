using System.Collections.Generic;
using UnityEngine;

public class Battle
{
    //public variables
    public List<Party> AttackingParties { get => attackingParties; set => attackingParties = value; }
    public List<Party> DefendingParties { get => defendingParties; set => defendingParties = value; }
    //public methods
    public bool BattleIndividual(Troop i_troopA,Troop i_troopB)
    {
        while(i_troopA.IsAlive && i_troopB.IsAlive)
        {
            // Play attack sound for troopA
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayAttackSound();
            }

            i_troopA.Health-=i_troopB.Damage;
            if(i_troopA.Health<=0)
            {
                i_troopA.IsAlive=false;
                i_troopA.Health=0;
                // Play death sound for troopA
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayDeathSound();
                }
            }
            else
            {
                // Play hit sound for troopA
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayHitSound();
                }
                // Play hit visual effect
                if (VisualEffectManager.Instance != null)
                {
                    VisualEffectManager.Instance.PlayHitEffect(i_troopA.transform.position);
                }
            }

            // Play attack sound for troopB
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayAttackSound();
            }

            i_troopB.Health-=i_troopA.Damage;
            if(i_troopB.Health<=0)
            {
                i_troopB.IsAlive=false;
                i_troopB.Health=0;
                // Play death sound for troopB
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayDeathSound();
                }
            }
            else
            {
                // Play hit sound for troopB
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayHitSound();
                }
                // Play hit visual effect
                if (VisualEffectManager.Instance != null)
                {
                    VisualEffectManager.Instance.PlayHitEffect(i_troopB.transform.position);
                }
            }
        }
        if(i_troopA.IsAlive)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    public void BattleParties(List<Party> i_attackingParties,List<Party> i_defendingParties)
    {
       foreach(var x in i_attackingParties)
       {
           foreach(var y in i_defendingParties)
           {
               foreach(var troopA in x.Troops)
               {
                   foreach(var troopB in y.Troops)
                   {
                       if(BattleIndividual(troopA,troopB))
                       {
                        y.Troops.Remove(troopB);
                       }
                       else
                       {
                        x.Troops.Remove(troopA);
                       }
                   }
               }
           }
       }
    }
    
    //private variables
    private List<Party> attackingParties;
    private List<Party> defendingParties;
}
