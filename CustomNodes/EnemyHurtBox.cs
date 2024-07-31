using Godot;
public sealed partial class EnemyHurtBox : Area2D
{
    public Node Parent => GetParent();

    public void ActivateCollisionsMasks()
    {
        switch (Parent)
        {
            case EnemyBase _:
                SetHurtBoxForRegularEnemy();
                break;
            case MeteorEnemyBase _:
                SetHurtBoxForMeteorEnemy();
                break;
            case LetterBlock _ or EnemyWord _ or AnswerMeteor _:
                SetHurtBoxForWordsEnemy();
                break;
            case AmmoBase _:
                break;
            default:
                GD.PrintErr("HurtBox parent is not recognized");
                break;
        }
    }

    public void DeactivateCollisionMasks()
    {
        this.ResetCollisionLayerAndMask();
    }

    private void SetHurtBoxForRegularEnemy()
    {
        this.ActivateCollisionLayer(CollisionLayers.RegularEnemyHurtBox);
    }

    private void SetHurtBoxForWordsEnemy()
    {
        this.ActivateCollisionLayer(CollisionLayers.WordEnemyHurtBox);
    }

    private void SetHurtBoxForMeteorEnemy()
    {
        this.ActivateCollisionLayer(CollisionLayers.MeteorEnemyHurtBox);
    }
}
