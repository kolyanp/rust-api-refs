using UnityEngine;
using UnityEngine.Tilemaps;

namespace FIMSpace.FTail;

[AddComponentMenu("FImpossible Creations/Hidden/Tail Collision Helper")]
public class TailCollisionHelper : MonoBehaviour
{
	public TailAnimator2 ParentTail;

	public Collider TailCollider;

	public Collider2D TailCollider2D;

	public int Index;

	private Transform previousCollision;

	internal Rigidbody RigBody { get; private set; }

	internal Rigidbody2D RigBody2D { get; private set; }

	internal TailCollisionHelper Init(bool addRigidbody = true, float mass = 1f, bool kinematic = false)
	{
		if ((Object)(object)TailCollider2D == (Object)null)
		{
			if (addRigidbody)
			{
				Rigidbody val = ((Component)this).GetComponent<Rigidbody>();
				if (!Object.op_Implicit((Object)(object)val))
				{
					val = ((Component)this).gameObject.AddComponent<Rigidbody>();
				}
				val.interpolation = (RigidbodyInterpolation)1;
				val.useGravity = false;
				val.isKinematic = kinematic;
				val.constraints = (RigidbodyConstraints)126;
				val.mass = mass;
				RigBody = val;
			}
			else
			{
				RigBody = ((Component)this).GetComponent<Rigidbody>();
				if (Object.op_Implicit((Object)(object)RigBody))
				{
					RigBody.mass = mass;
				}
			}
		}
		else if (addRigidbody)
		{
			Rigidbody2D val2 = ((Component)this).GetComponent<Rigidbody2D>();
			if (!Object.op_Implicit((Object)(object)val2))
			{
				val2 = ((Component)this).gameObject.AddComponent<Rigidbody2D>();
			}
			val2.interpolation = (RigidbodyInterpolation2D)1;
			val2.gravityScale = 0f;
			val2.isKinematic = kinematic;
			val2.constraints = (RigidbodyConstraints2D)7;
			val2.mass = mass;
			RigBody2D = val2;
		}
		else
		{
			RigBody2D = ((Component)this).GetComponent<Rigidbody2D>();
			if (Object.op_Implicit((Object)(object)RigBody2D))
			{
				RigBody2D.mass = mass;
			}
		}
		return this;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if ((Object)(object)ParentTail == (Object)null)
		{
			Object.Destroy((Object)(object)this);
			return;
		}
		TailCollisionHelper component = ((Component)collision.transform).GetComponent<TailCollisionHelper>();
		if ((!Object.op_Implicit((Object)(object)component) || (ParentTail.CollideWithOtherTails && !((Object)(object)component.ParentTail == (Object)(object)ParentTail))) && !ParentTail._TransformsGhostChain.Contains(collision.transform) && !ParentTail.IgnoredColliders.Contains(collision.collider))
		{
			ParentTail.CollisionDetection(Index, collision);
			previousCollision = collision.transform;
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if ((Object)(object)collision.transform == (Object)(object)previousCollision)
		{
			ParentTail.ExitCollision(Index);
			previousCollision = null;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.isTrigger && (!ParentTail.IgnoreMeshColliders || !(other is MeshCollider)) && !(other is CharacterController) && !ParentTail._TransformsGhostChain.Contains(((Component)other).transform) && !ParentTail.IgnoredColliders.Contains(other) && (ParentTail.CollideWithOtherTails || !Object.op_Implicit((Object)(object)((Component)((Component)other).transform).GetComponent<TailCollisionHelper>())))
		{
			ParentTail.AddCollider(other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (ParentTail.IncludedColliders.Contains(other) && !ParentTail.DynamicAlwaysInclude.Contains((Component)(object)other))
		{
			ParentTail.IncludedColliders.Remove(other);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!other.isTrigger && !(other is CompositeCollider2D) && !(other is TilemapCollider2D) && !(other is EdgeCollider2D) && !ParentTail._TransformsGhostChain.Contains(((Component)other).transform) && !ParentTail.IgnoredColliders2D.Contains(other) && (ParentTail.CollideWithOtherTails || !Object.op_Implicit((Object)(object)((Component)((Component)other).transform).GetComponent<TailCollisionHelper>())))
		{
			ParentTail.AddCollider(other);
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (ParentTail.IncludedColliders2D.Contains(other) && !ParentTail.DynamicAlwaysInclude.Contains((Component)(object)other))
		{
			ParentTail.IncludedColliders2D.Remove(other);
		}
	}
}
