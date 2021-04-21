# Project

Implementation in C# of a heuristic solution for the Unit Commitment problem

```
For a memory of design and development, please refer to the doc MEMORY.md
```

## Description

This solution makes no use of any off-the-shelf linear programming package or any other commercial library implementing known solutions of the Unit Commitment problem.

## Requirements

Please refer to the doc /requirements/README.md

### Initial Considerations

1. The power generation cost is linear, directly proportional to the number of MW produced, that is, no economy of scale
2. Wind generation is clean, efficient and cheap: use all wind capacity existing in the grid
3. The problem abstracts parameters that should be relevant in a realistic model: it assumes zero transmission costs, which should take into account the distance of consumers from powerplants (implying uniform distribution of consumers around grid providers and, moreover ignoring that proximity to a given producer does not mean that that producer can satisfy the local consumer demand in its totality)
4. The abstractions mentioned above make the problem a candidate to a linear solver, as the problem now can be described by a set of linear equations and inequations
5. However, my understanding is that if transmission costs and distances powerplants-consumers were taken into account then some sort of graph based model and graph algorithm would be more adequate
6. In case of network disruptions, we assume that another API request will require a new provisioning plan - so a look at the efficienty of the algorithm as a function of payload is important. We will try to minimize complexity as possible (actually, in our design we will make use of dynamic programming caching as a way to alleviate the costs of our solution; we will also indicate in the doc MEMORY.md other ways in which performance can be improved)
7. We thought about applying genetic programming to the problem, however, in order to fully comply with the "write an algorithm yourself" requirement, and given the overall complexity of fully developing and testing a gp solution, we opted for a simpler heuristic
8. Being a heuristic solution means we have used "rules of thumb" based on "good sense and reasonable assumptions", as explained in more detail in the doc MEMORY.md
9. So we make use exclusively of built-in c# class libraries, such as generic list collection

## User Guide

### How to run

Either

1. Run the UnitCommitment project in Visual Studio
2. Run the docker-compose project in Visual Studio (at least the first time)
3. Run the Unit Commitment container in Docker Dashboard

The application runs on HTPPS port 8888, endpoint is https://localhost:8888/productionplan

### How to configure

Fill in the UnitCommitment section of AppSettings.json file

```
  "UnitCommitment": {
    "ConsiderCo2": true,
    "Co2Value": 0.3
  }
```

1. ConsiderCo2 is true/false, enabling or disabling the consideration of Co2 emissions in the merits of powerplants
2. Co2Value is the amount of Co2 emitted per MW produced by the powerplant

### How to test

1. Load the file powerplant.postman_collection.json and run the tests (the provided payloads are in included)
2. Use Swagger (automatically loaded in the browser at startup) at https://localhost:8888/swagger/index.html
3. Unfortunately at this moment there are still no unit tests implemented in the Test project
