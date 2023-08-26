using Blog.Models;

namespace Blog.Data.Repository
{
	public class Repository : IRepository
	{
		private AppDbContext _ctx;
		public Repository(AppDbContext ctx)
		{
			_ctx = ctx;
		}
		public void AddPost(Post post)
		{
			_ctx.Post.Add(post);
		}

		public List<Post> GetAllPosts()
		{
			return _ctx.Post.ToList();
		}

		public Post GetPost(int id)
		{
			return _ctx.Post.FirstOrDefault(p => p.Id == id);
		}

		public void RemovePost(int id)
		{
			_ctx.Post.Remove(GetPost(id));
		}

		public void UpdatePost(Post post)
		{
			_ctx.Post.Update(post);
		}

		public async Task<bool> SaveChangesAsync()
		{
			if (await _ctx.SaveChangesAsync() > 0)
			{
				return true;
			}
			return false;
		}
	}
}
