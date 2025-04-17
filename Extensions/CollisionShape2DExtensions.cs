using Godot;

namespace AlfaEBetto.Extensions
{
	public static class CollisionObject2DExtensions
	{
		public static void ResetCollisionLayerAndMask(this CollisionObject2D collisionShape)
		{
			collisionShape.CollisionLayer = 0;
			collisionShape.CollisionMask = 0;
		}

		public static void ActivateCollisionLayer(this CollisionObject2D collisionShape, CollisionLayers layer)
		{
			collisionShape.SetCollisionLayerValue(
				(int)layer,
				true
			);
		}

		public static void DeactivateCollisionLayer(this CollisionObject2D collisionShape, CollisionLayers layer)
		{
			collisionShape.SetCollisionLayerValue(
				(int)layer,
				false
			);
		}

		public static void ActivateCollisionMask(this CollisionObject2D collisionShape, CollisionLayers layer)
		{
			collisionShape.SetCollisionMaskValue(
				(int)layer,
				true
			);
		}

		public static void DeactivateCollisionMask(this CollisionObject2D collisionShape, CollisionLayers layer)
		{
			collisionShape.SetCollisionMaskValue(
				(int)layer,
				false
			);
		}
	}
}
