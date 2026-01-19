
# Graduation Project Portal

A web-based system designed to manage and organize graduation projects at Jordan University of Science and Technology (JUST).

## Table of Contents
- [Overview](#overview)
- [Features](#features)
- [Technologies Used](#technologies-used)
- [Installation](#installation)
- [Usage](#usage)
- [License](#license)

## Overview

This system addresses common challenges in graduation project management, such as unequal supervision distribution, lack of transparency in approvals, and inefficient communication.

The platform provides an environment where:

1) Students can submit and manage graduation project requests

2) Supervisors can review and supervise assigned teams

3) Coordinators can oversee and regulate the entire process

## System Roles

### Students

1) Submit graduation project requests

2) View project status and supervisor feedback

3) Access project information

### Supervisors

1) Review and accept/reject supervision requests

2) Manage assigned student teams

3) Provide evaluations and feedback

### Coordinators

1) Oversee the graduation project process

2) Assign supervisors and manage team creation

3) Archive completed projects

## Features

1) Role-based control (Student, Supervisor, Coordinator)

2) Project and team management

3) Supervisor assignment and workload regulation

4) Project approval and tracking system

5) Real-time communication support

## Technologies Used

1) C#

2) ASP.NET Core

3) Entity Framework Core

4) SQLite Database

5) HTML, CSS, JavaScript

## Installation

1) Clone the repository:

git clone https://github.com/GORGE36/Graduation-Projects-Portal.git


2) Open the solution in Visual Studio

3) Restore dependencies:
 dotnet restore

5) Run the application: 
 dotnet run --project SuperSee

## Usage

1) Coordinator creates team with members and a supervisor

2) Coordinator can swap members between teams

3) Supervisors are notified and can accept and refuse team supervision

3) Supervisors manage approved teams

4) Student is shown his team-mates after supervisor approval

## License

This project is developed for academic purposes as part of the Graduation Project requirements at Jordan University of Science and Technology.
