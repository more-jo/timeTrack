﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace timeTrack
{
  public class TaskItem
  {    
    public string TaskName { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public TimeSpan DurationSession { get; set;}
    public TimeSpan DurationTaskTotalDay { get; set; }
    public TimeSpan DurationAllTasksDay { get; set; }
  }
}