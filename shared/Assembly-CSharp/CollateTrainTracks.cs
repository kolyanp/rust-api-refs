using UnityEngine;

public class CollateTrainTracks : ProceduralComponent
{
	private const float MAX_NODE_DIST = 0.1f;

	private const float MAX_NODE_DIST_SQR = 0.010000001f;

	private const float MAX_NODE_ANGLE = 10f;

	public override bool RunOnCache => true;

	public override void Process(uint seed)
	{
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		TrainTrackSpline[] array = Object.FindObjectsByType<TrainTrackSpline>((FindObjectsSortMode)0);
		TrainTrackSpline[] array2;
		for (int num = array.Length - 1; num >= 0; num--)
		{
			TrainTrackSpline ourSpline = array[num];
			if (ourSpline.dataIndex < 0 && ourSpline.points.Length > 3)
			{
				int nodeIndex;
				for (nodeIndex = ourSpline.points.Length - 2; nodeIndex >= 1; nodeIndex--)
				{
					Vector3 ourPos = ourSpline.points[nodeIndex];
					Vector3 ourTangent = ourSpline.tangents[nodeIndex];
					array2 = array;
					foreach (TrainTrackSpline trainTrackSpline in array2)
					{
						if (!((Object)(object)ourSpline == (Object)(object)trainTrackSpline))
						{
							Vector3 startPointWorld = trainTrackSpline.GetStartPointWorld();
							Vector3 endPointWorld = trainTrackSpline.GetEndPointWorld();
							Vector3 startTangentWorld = trainTrackSpline.GetStartTangentWorld();
							Vector3 endTangentWorld = trainTrackSpline.GetEndTangentWorld();
							if (!CompareNodes(startPointWorld, startTangentWorld) && !CompareNodes(endPointWorld, endTangentWorld) && !CompareNodes(startPointWorld, -startTangentWorld))
							{
								CompareNodes(endPointWorld, -endTangentWorld);
							}
						}
					}
					bool CompareNodes(Vector3 theirPos, Vector3 theirTangent)
					{
						//IL_0002: Unknown result type (might be due to invalid IL or missing references)
						//IL_0007: Unknown result type (might be due to invalid IL or missing references)
						//IL_000a: Unknown result type (might be due to invalid IL or missing references)
						//IL_000f: Unknown result type (might be due to invalid IL or missing references)
						//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
						//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
						//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
						//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
						//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
						//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
						//IL_0113: Unknown result type (might be due to invalid IL or missing references)
						//IL_0118: Unknown result type (might be due to invalid IL or missing references)
						if (NodesConnect(ourPos, theirPos, ourTangent, theirTangent))
						{
							TrainTrackSpline trainTrackSpline2 = ((Component)ourSpline).gameObject.AddComponent<TrainTrackSpline>();
							Vector3[] array4 = (Vector3[])(object)new Vector3[ourSpline.points.Length - nodeIndex];
							Vector3[] array5 = (Vector3[])(object)new Vector3[ourSpline.points.Length - nodeIndex];
							Vector3[] array6 = (Vector3[])(object)new Vector3[nodeIndex + 1];
							Vector3[] array7 = (Vector3[])(object)new Vector3[nodeIndex + 1];
							for (int num2 = ourSpline.points.Length - 1; num2 >= 0; num2--)
							{
								if (num2 >= nodeIndex)
								{
									array4[num2 - nodeIndex] = ourSpline.points[num2];
									array5[num2 - nodeIndex] = ourSpline.tangents[num2];
								}
								if (num2 <= nodeIndex)
								{
									array6[num2] = ourSpline.points[num2];
									array7[num2] = ourSpline.tangents[num2];
								}
							}
							ourSpline.SetAll(array6, array7, ourSpline);
							trainTrackSpline2.SetAll(array4, array5, ourSpline);
							nodeIndex--;
							return true;
						}
						return false;
					}
				}
			}
		}
		array = Object.FindObjectsByType<TrainTrackSpline>((FindObjectsSortMode)0);
		array2 = array;
		foreach (TrainTrackSpline ourSpline2 in array2)
		{
			Vector3 ourStartPos = ourSpline2.GetStartPointWorld();
			Vector3 ourEndPos = ourSpline2.GetEndPointWorld();
			Vector3 ourStartTangent = ourSpline2.GetStartTangentWorld();
			Vector3 ourEndTangent = ourSpline2.GetEndTangentWorld();
			if (NodesConnect(ourStartPos, ourEndPos, ourStartTangent, ourEndTangent))
			{
				ourSpline2.AddTrackConnection(ourSpline2, TrainTrackSpline.TrackPosition.Next, TrainTrackSpline.TrackOrientation.Same);
				ourSpline2.AddTrackConnection(ourSpline2, TrainTrackSpline.TrackPosition.Prev, TrainTrackSpline.TrackOrientation.Same);
				continue;
			}
			TrainTrackSpline[] array3 = array;
			foreach (TrainTrackSpline otherSpline in array3)
			{
				Vector3 theirStartPos;
				Vector3 theirEndPos;
				Vector3 theirStartTangent;
				Vector3 theirEndTangent;
				if (!((Object)(object)ourSpline2 == (Object)(object)otherSpline))
				{
					theirStartPos = otherSpline.GetStartPointWorld();
					theirEndPos = otherSpline.GetEndPointWorld();
					theirStartTangent = otherSpline.GetStartTangentWorld();
					theirEndTangent = otherSpline.GetEndTangentWorld();
					if (!CompareNodes2(ourStart: false, theirStart: true) && !CompareNodes2(ourStart: false, theirStart: false) && !CompareNodes2(ourStart: true, theirStart: true))
					{
						CompareNodes2(ourStart: true, theirStart: false);
					}
				}
				bool CompareNodes2(bool ourStart, bool theirStart)
				{
					//IL_000c: Unknown result type (might be due to invalid IL or missing references)
					//IL_0004: Unknown result type (might be due to invalid IL or missing references)
					//IL_001d: Unknown result type (might be due to invalid IL or missing references)
					//IL_0015: Unknown result type (might be due to invalid IL or missing references)
					//IL_0022: Unknown result type (might be due to invalid IL or missing references)
					//IL_0031: Unknown result type (might be due to invalid IL or missing references)
					//IL_0028: Unknown result type (might be due to invalid IL or missing references)
					//IL_0036: Unknown result type (might be due to invalid IL or missing references)
					//IL_0045: Unknown result type (might be due to invalid IL or missing references)
					//IL_003c: Unknown result type (might be due to invalid IL or missing references)
					//IL_004a: Unknown result type (might be due to invalid IL or missing references)
					//IL_005b: Unknown result type (might be due to invalid IL or missing references)
					//IL_005c: Unknown result type (might be due to invalid IL or missing references)
					//IL_005d: Unknown result type (might be due to invalid IL or missing references)
					//IL_004f: Unknown result type (might be due to invalid IL or missing references)
					//IL_0055: Unknown result type (might be due to invalid IL or missing references)
					//IL_005a: Unknown result type (might be due to invalid IL or missing references)
					Vector3 ourPos2 = (ourStart ? ourStartPos : ourEndPos);
					Vector3 ourTangent2 = (ourStart ? ourStartTangent : ourEndTangent);
					Vector3 theirPos = (theirStart ? theirStartPos : theirEndPos);
					Vector3 val = (theirStart ? theirStartTangent : theirEndTangent);
					if (ourStart == theirStart)
					{
						val *= -1f;
					}
					if (NodesConnect(ourPos2, theirPos, ourTangent2, val))
					{
						if (ourStart)
						{
							ourSpline2.AddTrackConnection(otherSpline, TrainTrackSpline.TrackPosition.Prev, theirStart ? TrainTrackSpline.TrackOrientation.Reverse : TrainTrackSpline.TrackOrientation.Same);
						}
						else
						{
							ourSpline2.AddTrackConnection(otherSpline, TrainTrackSpline.TrackPosition.Next, (!theirStart) ? TrainTrackSpline.TrackOrientation.Reverse : TrainTrackSpline.TrackOrientation.Same);
						}
						if (theirStart)
						{
							otherSpline.AddTrackConnection(ourSpline2, TrainTrackSpline.TrackPosition.Prev, ourStart ? TrainTrackSpline.TrackOrientation.Reverse : TrainTrackSpline.TrackOrientation.Same);
						}
						else
						{
							otherSpline.AddTrackConnection(ourSpline2, TrainTrackSpline.TrackPosition.Next, (!ourStart) ? TrainTrackSpline.TrackOrientation.Reverse : TrainTrackSpline.TrackOrientation.Same);
						}
						return true;
					}
					return false;
				}
			}
		}
		static bool NodesConnect(Vector3 val, Vector3 theirPos, Vector3 val2, Vector3 theirTangent)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if (Vector3.SqrMagnitude(val - theirPos) < 0.010000001f)
			{
				return Vector3.Angle(val2, theirTangent) < 10f;
			}
			return false;
		}
	}
}
