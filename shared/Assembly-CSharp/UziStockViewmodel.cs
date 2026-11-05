using System;
using UnityEngine;

public class UziStockViewmodel : MonoBehaviour, IClientComponent, IViewmodelComponent
{
	[Serializable]
	public struct AdsSettings
	{
		public IronsightAimPoint AimPoint;

		public float ZoomFactor;

		public float IntroSpeed;

		public float OutroSpeed;

		public LazyAimProperties LazyAim;
	}

	public Animator TargetAnimator;

	public IronSights IronSights;

	[Tooltip("Used whilst the stock is collapsed, even with a sight attached.")]
	public AdsSettings StockCollapsedSettings;

	[Tooltip("Used whilst the stock is extended/deployed.")]
	public AdsSettings StockExtendedSettings;
}
