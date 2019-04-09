using RacingWebScrape.Models.Courses;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RacingWebScrape.Interfaces
{
    public interface ICourseRepository
    {
        void Add(Course course);
        void Update(Course course);
        void Delete(Course course);

        Course Get(int id);
        IEnumerable<Course> Get();
    }
}
