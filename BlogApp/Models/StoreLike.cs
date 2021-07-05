using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogApp.Models
{
    public class StoreLike
    {
        //public StoreLike(int userid,int blogpostid)
        //{
        //    UserId = userid;
        //    BlogPostId = blogpostid;
        //}

        public int StoreLikeId { get; set; }
        public int BlogPostId { get; set; }
        public int UserId { get; set; }

    }
}
