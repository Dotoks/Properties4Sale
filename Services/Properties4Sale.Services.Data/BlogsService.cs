﻿namespace Properties4Sale.Services.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Hosting;

    using Properties4Sale.Data.Common.Repositories;
    using Properties4Sale.Data.Models;
    using Properties4Sale.Services.Mapping;
    using Properties4Sale.Web.ViewModels.Blog;

    public class BlogsService : IBlogsService
    {
        private readonly IDeletableEntityRepository<Blog> blogRepository;

        public BlogsService(IDeletableEntityRepository<Blog> blogRepository)
        {
            this.blogRepository = blogRepository;
        }

        public async Task CreateAsync(CreateBlogInputModel input, string userId, string imagePath)
        {
            var blog = new Blog
            {
                Name = input.Name,
                SubName = input.SubName,
                Author = input.Author,
                Category = input.Category,
                Date = input.Date,
                Description = input.Description,
                AddedByUserId = userId,
            };

            var allowedExtensions = new[] { "jpg", "png", "gif" };

            Directory.CreateDirectory($"{imagePath}/blogs");

            foreach (var image in input.BlogImages)
            {
                var extension = Path.GetExtension(image.FileName).TrimStart('.');

                if (!allowedExtensions.Any(x => extension.EndsWith(x)))
                {
                    throw new Exception($"Invalid Image Extension {extension}");
                }

                var dbImage = new BlogImage
                {
                    AddedByUserid = userId,
                    Extension = extension,
                };
                blog.BlogImages.Add(dbImage);

                var phycicalPath = $"{imagePath}/blogs/{dbImage.Id}.{extension}";

                using Stream fileStream = new FileStream(phycicalPath, FileMode.Create);
                await image.CopyToAsync(fileStream);
            }

            await this.blogRepository.AddAsync(blog);
            await this.blogRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var blog = this.blogRepository.All().FirstOrDefault(x => x.Id == id);
            this.blogRepository.Delete(blog);
            await this.blogRepository.SaveChangesAsync();
        }

        public IEnumerable<T> GetAll<T>(int page, int itemsPerPage = 12)
        {
            var blogs = this.blogRepository.AllAsNoTracking()
             .OrderByDescending(x => x.Id)
             .Skip((page - 1) * itemsPerPage)
             .Take(itemsPerPage)
             .To<T>()
             .ToList();

            return blogs;
        }

        public IEnumerable<T> GetBlogs<T>(int count)
        {
            var blogs = this.blogRepository.AllAsNoTracking().OrderBy(x => x.Id).Take(count).To<T>().ToList();

            return blogs;
        }

        public T GetById<T>(int id)
        {
            var blog = this.blogRepository.AllAsNoTracking().Where(x => x.Id == id).To<T>().FirstOrDefault();

            return blog;
        }

        public int GetCount()
        {
            return this.blogRepository.AllAsNoTracking().Count();
        }

        public async Task UpdateAsync(int id, EditBlogInputModel input)
        {
            var blog = this.blogRepository.All().FirstOrDefault(x => x.Id == id);
            blog.Name = input.Name;
            blog.Description = input.Description;
            blog.SubName = input.SubName;
            blog.Category = input.Category;
            blog.Author = input.Author;
            blog.Date = input.Date;

            await this.blogRepository.SaveChangesAsync();
        }
    }
}
