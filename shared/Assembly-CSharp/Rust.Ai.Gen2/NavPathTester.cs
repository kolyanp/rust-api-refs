using UnityEngine;

namespace Rust.Ai.Gen2;

[RequireComponent(typeof(RustNavMeshAgent))]
public class NavPathTester : MonoBehaviour
{
	[SerializeField]
	private NavPathTestType testType;

	[SerializeField]
	private Transform target;

	[SerializeField]
	private float sampleRadius = 10f;

	[SerializeField]
	private LayerMask traceMask = LayerMask.op_Implicit(1218519041);

	public NavPathTester()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
	}
}
