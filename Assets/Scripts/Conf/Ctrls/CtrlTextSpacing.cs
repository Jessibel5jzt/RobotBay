using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace WestBay
{
	public class CtrlTextSpacing : BaseMeshEffect
	{
		public float TextSpacing;

		public override void ModifyMesh(VertexHelper vh)
		{
			if (!IsActive() || vh.currentVertCount == 0)
			{
				return;
			}
			List<UIVertex> vertexs = new List<UIVertex>();
			vh.GetUIVertexStream(vertexs);
			int indexCount = vh.currentIndexCount;
			UIVertex vt;
			for (int i = 6; i < indexCount; i++)
			{
				//第一个字不用改变位置
				vt = vertexs[i];
				vt.position += new Vector3(TextSpacing * (i / 6), 0, 0);
				vertexs[i] = vt;
				//以下注意点与索引的对应关系
				if (i % 6 <= 2)
				{
					vh.SetUIVertex(vt, (i / 6) * 4 + i % 6);
				}
				if (i % 6 == 4)
				{
					vh.SetUIVertex(vt, (i / 6) * 4 + i % 6 - 1);
				}
			}
		}
	}
}