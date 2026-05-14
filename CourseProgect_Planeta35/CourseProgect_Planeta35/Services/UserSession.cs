using CourseProgect_Planeta35.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProgect_Planeta35.Services
{
    public static class UserSession
    {
        public static User CurrentUser { get; set; }
    }
}
