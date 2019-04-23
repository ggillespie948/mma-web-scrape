using Microsoft.EntityFrameworkCore;
using RacingWebScrape.Db;
using RacingWebScrape.Interfaces;
using RacingWebScrape.Models.Courses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RacingWebScrape.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly RacingDbContext _context;
        public CourseRepository(RacingDbContext context)
        {
            _context = context;
        }

        public void Add(Course course)
        {
            _context.Courses.Add(course);
        }

        public void Delete(Course course)
        {
            _context.Entry(course).State = EntityState.Deleted;
        }

        public Course Get(int id)
        {
            return _context.Courses
                .Where(i => i.Id == id)
                .FirstOrDefault();
        }

        public IEnumerable<Course> Get()
        {
            return _context.Courses.ToList();
        }

        public Course GetByName(string name)
        {
            return _context.Courses.Where(i => i.Name == name).FirstOrDefault();
        }

        public void Update(Course course)
        {
            _context.Entry(course).State = EntityState.Modified;
        }
    }
}
