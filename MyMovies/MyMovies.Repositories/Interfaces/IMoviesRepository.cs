﻿using MyMovies.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyMovies.Repositories.Interfaces
{
    public interface IMoviesRepository : IBaseRepository<Movie>
    {
        List<Movie> GetByTitle(string title);


    }
}
