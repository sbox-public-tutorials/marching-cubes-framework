public class OctTree<T>
{
	public T Data;

	public OctTree<T> parent { get; set; }

	public OctTree<T> child000 { get; set; }
	public OctTree<T> child001 { get; set; }
	public OctTree<T> child010 { get; set; }
	public OctTree<T> child011 { get; set; }

	public OctTree<T> child100 { get; set; }
	public OctTree<T> child101 { get; set; }
	public OctTree<T> child110 { get; set; }
	public OctTree<T> child111 { get; set; }

	public OctTree()
	{

	}

	public OctTree(T data)
	{
		Data = data;
	}
}
