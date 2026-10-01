using UnityEngine;

[RequireComponent(typeof(ItemModWearable))]
public class ItemModPaintable : ItemModAssociatedEntity<PaintedItemStorageEntity>
{
	public static readonly Phrase ItemPaintTitle = new Phrase("item.paint", "Paint");

	public static readonly Phrase ItemPaintDesc = new Phrase("item.paint.desc", "Paint on this item.");

	public GameObjectRef ChangeSignTextDialog;

	public MeshPaintableSource[] PaintableSources;

	protected override bool AllowNullParenting => true;

	protected override bool OwnedByParentPlayer => true;

	static ItemModPaintable()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected Obj, but got Unknown
	}
}
