private Solid SolidoDaInterseccao(Element ele, List<Element> referenciasFiltradas, Document doc)
{

	List<Solid> lsolids = new List<Solid>();

	#region PEGA OS SÓLIDOS DE CADA ELEMENTO SELECIONADO

	if (referenciasFiltradas != null)
	{
		//Opções da geometria
		Options gOptions = new Options();
		gOptions.DetailLevel = ViewDetailLevel.Fine;

		// Verifica quais colunas interseccionam com a parede através de um Outline Filter
		BoundingBoxXYZ bb = ele.get_BoundingBox(null);
		Outline wallOutline = new Outline(bb.Min, bb.Max);

		foreach (var item in referenciasFiltradas)
		{
			// Verifica se o elemento selecionado é um link
			RevitLinkInstance linkInstance = item as RevitLinkInstance;

			if (linkInstance != null)
			{
				Document linkedDoc = linkInstance.GetLinkDocument();

				Transform transform = linkInstance.GetTransform();

				ICollection<Element> lstLinkElements = Utilities.CollectorUtilities.CollectorAllElementsOfCategory(linkedDoc, BuiltInCategory.OST_StructuralColumns);

				foreach (Element linkEle in lstLinkElements)
				{
					BoundingBoxXYZ bbLinkEle = linkEle.get_BoundingBox(doc.ActiveView);
					XYZ minPoint = bbLinkEle.Min;
					XYZ maxPoint = bbLinkEle.Max;

					// Verifica a transformação do arquivo linkado e caso tenha sido alterado aplica nos elementos
					bool notTransformed = transform.AlmostEqual(Transform.Identity);

					if (!notTransformed)
					{
						minPoint = transform.OfPoint(minPoint);
						maxPoint = transform.OfPoint(maxPoint);
					}

					Outline bbLinkOutline = new Outline(minPoint, maxPoint);

					if (wallOutline.Intersects(bbLinkOutline, 0))
					{
						if (notTransformed)
						{
							lsolids.AddRange(GetElementSolid(linkEle, gOptions));
						}
						else
						{
							lsolids.AddRange(GetElementSolidTransformed(linkEle, gOptions, transform));
						}
					}
				}
			}
			else
			{
				lsolids.AddRange(GetElementSolid(item, gOptions));
			}

		}
	}

	#endregion

	#region UNE OS SÓLIDOS EM UM ÚNICO SÓLIDO

	Solid joinedSolid = null;

	if (lsolids.Count > 1)
	{
		joinedSolid = BooleanOperationsUtils.ExecuteBooleanOperation(lsolids[0], lsolids[1], BooleanOperationsType.Union);

		lsolids.RemoveRange(0, 2);

		if (lsolids.Count > 0)
		{
			for (int i = 0; i < lsolids.Count; i++)
			{
				Solid joinedSolid2 = BooleanOperationsUtils.ExecuteBooleanOperation(joinedSolid, lsolids[i], BooleanOperationsType.Union);
				joinedSolid = joinedSolid2;
			}
		}
	}
	else
	{
		joinedSolid = lsolids[0];
	}

	#endregion

	return joinedSolid;

}

private List<Solid> GetElementSolid(Element item, Options gOptions)
{
	GeometryElement geom = item.get_Geometry(gOptions);

	Solid gSolid = null;

	List<Solid> lsolids = new List<Solid>();

	foreach (GeometryObject gObj in geom)
	{
		GeometryInstance instance = gObj as GeometryInstance;

		if (instance != null)
		{
			GeometryElement gEle = instance.GetInstanceGeometry();

			foreach (GeometryObject gObj2 in gEle)
			{
				gSolid = gObj2 as Solid;

				if (gSolid.Volume != 0)
				{
					lsolids.Add(gSolid);
				}
			}
		}
		else
		{
			gSolid = gObj as Solid;

			if (gSolid.Volume != 0)
			{
				lsolids.Add(gSolid);
			}
		}
	}

	return lsolids;
}

private List<Solid> GetElementSolidTransformed(Element item, Options gOptions, Transform transform)
{
            GeometryElement geom = item.get_Geometry(gOptions);

            Solid gSolid = null;

            List<Solid> lsolids = new List<Solid>();

            foreach (GeometryObject gObj in geom)
            {
                GeometryInstance instance = gObj as GeometryInstance;

                if (instance != null)
                {
                    GeometryElement gEle = instance.GetInstanceGeometry();

                    foreach (GeometryObject gObj2 in gEle)
                    {
                        gSolid = gObj2 as Solid;

                        if (gSolid.Volume != 0)
                        {
                            gSolid = SolidUtils.CreateTransformed(gSolid, transform);

                            lsolids.Add(gSolid);
                        }
                    }
                }
                else
                {
                    gSolid = gObj as Solid;

                    if (gSolid.Volume != 0)
                    {
                        gSolid = SolidUtils.CreateTransformed(gSolid, transform);

                        lsolids.Add(gSolid);
                    }
                }
            }

            return lsolids;
        }
