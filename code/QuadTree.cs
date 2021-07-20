public class QuadTree<T>
{
	public T Data;

	public QuadTree<T> parent { get; set; }

	public QuadTree<T> child00 { get; set; }
	public QuadTree<T> child01 { get; set; }
	public QuadTree<T> child10 { get; set; }
	public QuadTree<T> child11 { get; set; }

	public QuadTree()
	{

	}

	public QuadTree( T data)
	{
		Data = data;
	}
}
