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
				//��һ���ֲ��øı�λ��
				vt = vertexs[i];
				vt.position += new Vector3(TextSpacing * (i / 6), 0, 0);
				vertexs[i] = vt;
				//����ע����������Ķ�Ӧ��ϵ
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