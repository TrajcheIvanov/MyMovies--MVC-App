﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyMovies.Common.Exceptions;
using MyMovies.Mappings;
using MyMovies.Models;
using MyMovies.Services.Interfaces;
using MyMovies.ViewModels;
using System;
using System.Linq;

namespace MyMovies.Controllers
{
    [Authorize]
    public class MoviesController : Controller
    {
        private IMoviesService _service { get; set; }

        public MoviesController(IMoviesService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        public IActionResult Overview(string title)
        {
            var movies = _service.GetRecipesByTitle(title);

            var moviesOverviewModels = movies.Select(x => x.ToOverviewModel()).ToList();
            return View(moviesOverviewModels);
        }
        
        public IActionResult ManageMovies(string errorMEssage, string successMessage, string updateMessage)
        {
            ViewBag.ErrorMessage = errorMEssage;
            ViewBag.SuccessMessage = successMessage;
            ViewBag.UpdateMEssage = updateMessage;
            var movies = _service.GetAllMovies();

            var manageModels = movies.Select(x => x.ToManageMovieModel()).ToList();
            return View(manageModels);
        }
        public IActionResult Details(int id)
        {
            try
            {
                var movie = _service.GetMovieById(id);

                if (movie == null)
                {
                    return RedirectToAction("ErrorNotFound", "Info");
                }

                return View(movie.ToDetailsModel());
            }
            catch (Exception)
            {

                return RedirectToAction("ErrorGeneral","Info");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(MovieCreateModel movie)
        {
            if (ModelState.IsValid)
            {
                var domainModel = movie.ToModel();
                _service.CreateMovie(domainModel);
                return RedirectToAction("ManageMovies", new { SuccessMessage = "Movie added sucessfully"});
            }

            return View(movie);
        }

        public IActionResult Delete(int Id)
        {
            try
            {
                _service.Delete(Id);
                return RedirectToAction("ManageMovies", new { SuccessMessage = "Movie deleted sucessfully" });
            }
            catch (NotFoundException ex)
            {

                return RedirectToAction("ManageMovies", new { ErrorMessage = ex.Message });
            }
            catch (Exception ex)
            {
                return RedirectToAction("InternalError", "Info");
            }
        }

        [HttpGet]
        public IActionResult Update(int Id)
        {
            try
            {
               var movie = _service.GetMovieById(Id);

                if (movie != null)
                {
                    return View(movie.ToUpdateModel());
                } 
                else
                {
                    throw new NotFoundException($"The Movie with Id {Id} was not found");
                }
               
            }
            catch (NotFoundException ex)
            {

                return RedirectToAction("ManageMovies", new { ErrorMessage = ex.Message });
            }
            catch (Exception ex)
            {
                return RedirectToAction("InternalError", "Info");
            }
        }

        public IActionResult EditMovie(Movie movie)
        {
            return View(movie);
        }

        [HttpPost]
        public IActionResult Update(MovieUpdateModel movie)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _service.Update(movie.ToModel());
                    return RedirectToAction("ManageMovies", new { UpdateMEssage = "Movie updated sucessfully" });
                }
                catch (NotFoundException ex)
                {

                    return RedirectToAction("ManageMovies", new { ErrorMessage = ex.Message });
                }
                catch (Exception)
                {
                    return RedirectToAction("InternalError", "Info");
                }

                
            }
            else
            {
                return RedirectToAction("InternalError", "Info");
            }
        }
    }
}
