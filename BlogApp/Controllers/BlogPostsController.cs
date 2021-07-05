using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogApp.Data;
using BlogApp.Models;
using Microsoft.AspNetCore.Identity;
using BlogApp.Migrations;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;

namespace BlogApp.Controllers
{
    public class BlogPostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BlogAppUser> _userManager;
        private readonly IDistributedCache _cache;
        public BlogPostsController(ApplicationDbContext context
            ,UserManager<BlogAppUser> userManager, IDistributedCache cache)
        {
            _context = context;
            _userManager = userManager;
            _cache = cache;
        }

        // GET: BlogPosts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.BlogPosts.Include(b => b.BlogAppUser).Include(b => b.Category);
            return View(await applicationDbContext.ToListAsync());
        }
        public async Task<string> GetLike(int id)
        {
            var getlikes = await _cache.GetAsync("like_"+ id);            
            if(getlikes == null)
            {
                var blogPost = _context.BlogPosts.ToList().Find(u => u.Id == id);
                return blogPost.Post_Like.ToString();
            }
            string serializeLikeCount = Encoding.UTF8.GetString(getlikes);
            var LikeCount = JsonConvert.DeserializeObject<string>(serializeLikeCount);
            return LikeCount;
        }
        public async Task<bool> ISLike(int id)
        {
            var GetuserId = _userManager.GetUserId(HttpContext.User);
            var userID = Int32.Parse(GetuserId);
            var isLike = _context.storeLikes.ToList().Where(u => u.UserId == userID && u.BlogPostId == id).Any();
            return isLike;
        }
        [HttpPost]
        public async  Task<string> Like(int id)
        {
            var GetuserId = _userManager.GetUserId(HttpContext.User);
            var userID = Int32.Parse(GetuserId);
            var blogPost = _context.BlogPosts.ToList().Find(u => u.Id == id);
            var storeLike = new StoreLike
            {
                UserId = userID,
                BlogPostId = blogPost.Id
            };
            if (!_context.storeLikes.ToList().Where(u => u.UserId == userID && u.BlogPostId == blogPost.Id).Any())
            {
                blogPost.Post_Like += 1;
                _context.storeLikes.Add(storeLike);
                
                 await _context.SaveChangesAsync();
                var LikeCountEncode = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(blogPost.Post_Like));
                await _cache.SetAsync("like_"+blogPost.Id, LikeCountEncode, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromDays(1) });
                var getlikes = await _cache.GetAsync("like_"+blogPost.Id);
                string serializeLikeCount = Encoding.UTF8.GetString(getlikes);
                var LikeCount = JsonConvert.DeserializeObject<string>(serializeLikeCount);
                return LikeCount;
            }
            
            blogPost.Post_Like -= 1;
            var deletedStoreLike = _context.storeLikes.FirstOrDefault(u => u.UserId == userID && u.BlogPostId == blogPost.Id);
            _context.storeLikes.Remove(deletedStoreLike);
            await _context.SaveChangesAsync();
            var content = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(blogPost.Post_Like));
            await _cache.SetAsync("like_"+blogPost.Id , content, new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromDays(1) });
            var likes = await _cache.GetAsync("like_"+blogPost.Id);
            string serializeRemoveLikeCount = Encoding.UTF8.GetString(likes);
            var RemoveLikeCount = JsonConvert.DeserializeObject<string>(serializeRemoveLikeCount);
            return RemoveLikeCount;
        }
        // GET: BlogPosts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }
            var blogpost= await _context.BlogPosts
                .Include(b => b.BlogAppUser)
                .Include(b => b.Category)
                .Include(b => b.BlogComments)
                .FirstOrDefaultAsync(m => m.Id == id);
            BlogDetailViewModel blogViewModel = new BlogDetailViewModel { blogPost = blogpost }; 
            //ViewBag.LikeCount = _context.BlogPosts.Where(u => u.Id == id).Select(u => u.Post_Like);
            

            return View(blogViewModel);
        }
        public async Task<IActionResult> AddComment(int blogid,BlogComment comment)
        {
            //if (id == null)
            //{
            //    return NotFound();
            //}
            if (ModelState.IsValid)
            {
                //comment.Id = 0;
                comment.BlogPostId = blogid;
                _context.Add(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            
            return View("Details");
        }
        public async Task<IActionResult> ShowUserPost(int? Id)
        {
            if(Id== null)
            {
                return NotFound();
            }
            var UserBlogPost = await _context.BlogPosts
                .Include(b => b.BlogAppUser)
                .Where(b => b.BlogAppUser.Id == Id).ToListAsync();
            
            return View(UserBlogPost);
        }
        // GET: BlogPosts/Create
        public IActionResult Create()
        {
            ViewData["userid"] = _userManager.GetUserName(HttpContext.User);
            //ViewData["UserId"] = new SelectList(_userManager.GetUserId(HttpContext.User), "UserId", "UserId");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Title");
            return View();
        }

        // POST: BlogPosts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Post_Like,CategoryId,UserId")] BlogPost blogPost)
        {
            if (ModelState.IsValid)
            {
                blogPost.Post_Like = 0;
                var userId =  _userManager.GetUserId(HttpContext.User);
                blogPost.UserId = Int32.Parse(userId);
                
                _context.Add(blogPost);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId",blogPost.UserId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", blogPost.CategoryId);
            return View(blogPost);
        }

        // GET: BlogPosts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", blogPost.UserId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", blogPost.CategoryId);
            return View(blogPost);
        }

        // POST: BlogPosts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,CategoryId,UserId")] BlogPost blogPost)
        {
            if (id != blogPost.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(blogPost);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogPostExists(blogPost.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", blogPost.UserId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", blogPost.CategoryId);
            return View(blogPost);
        }

        // GET: BlogPosts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blogPost = await _context.BlogPosts
                .Include(b => b.BlogAppUser)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
        }

        // POST: BlogPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            _context.BlogPosts.Remove(blogPost);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BlogPostExists(int id)
        {
            return _context.BlogPosts.Any(e => e.Id == id);
        }
    }
}
