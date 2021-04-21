# Memory of Development

/// TO BE COMPLETED

## General Info

The first thing to be clear was the strategy of making use of all wind capacity first, since this mode of power generation is cheap, efficient and clean.

In order to select additional "expensive, inefficient and dirty" power sources, at first we decided to implement some sort naive heuristic, based on ordering elements by their weights (fuels, co2) and cherrypicking the better ones. Given that we are dealing with a continous space, we considered a few ways of discretizing it.

One possible way would be to split gasfired powerplants into two, one capable of generating min capacity and the other capable of generating max capacity, with a margin of flexibility equal to (min, max].

Let's call this the naive method.

Another idea would be to actually fragment the power capacity of each powerplant is a N-tuple of power levels, so that each powerplant would generate power at one out of N possible levels, for example, for N=5, (min, 25%, 50%, 75%, 100%), whereas min would assume the value zero for those plants with min restriction, and the % levels would be calculated as fractions of the range [min, max].

The subjacent idea of the second potential strategy is the "sum of infinitesimals". That is, by fragmenting the Real segment [min, max] in N points, we would expect to be able to select the level closer to the optimal choice - the final solution would require some adjustment from the arbitratrily defined point selected by the algorithm to the exact production level which would make the combine production of all powerplants match the marked demand _exactly_.

The problem with fragmenting in N-tuples is obviously the explosion of combinations - N possibilities for each powerplant. We investigated if would be possible to tame the combinatorics explosion through some smart algorithm, let's say by discarding as many unfeasible combinations as early as possible in the process. However, even for small N spaces, the number of combinations would require the solution to make use of distributed computation, that is, to delegate portions of the N ^ number-of-powerplants combinations to subordinated sub-processes.

Lets's call this the combinatorics method.

So, we investigated and explored these and other ideas - e.g. partitions of integers, subset sums, multisets - until finally returning back to a more simple concept, closer to the initial naive method. We looked at some variants of the knapsack problem and chose to use the greedy fractional knapsack, which basically selects whole elements like the traditional knapsack but allowing the last selection to be fractioned in order to meet an exact sum.

In our case we had also to deal with minimum limitations, but we handled it introducing the concept of surplus: we allow powerplants to be selected by min, whenever min is less than remaining demand - this way preserving the selection of the best ranked powerplants - and we add an additional step to fix the surplus (min > remaining demand). During the correction step we move backwards through the ranking, trying to accomodate the surplus by reducing the previously committed generation level of the other (and best ranked) powerplants - this way meeting the _exact_ demand while preserving the use of the best ranked plants.

About the ranking, which implements the idea of merit: first, as said, we select _all_ capacity - wind efficiency considered - of all windturbine generators and we don't touch it anymore. This is another aspect in which our algorithm is greedy, we assume we can find a solution by using 100% of wind power.

By the way, after the payload request is received we create a list of non windpower powerplants ranked by merit, merit being a ration between the cost of MW generation (taking into account factors like efficiency, fuel cost, and, if configured, penalizing the cost with co2 emissions) and the max power capacity.

Given that this is an heuristic method, we use this ratio, calculated as is, by the max capacity, throughout the whole calculation. That is, we define the performance merit as greedy capacity / unit cost; we do not define performance as a function of effetive power and we do not try to normalize capacities or ratios for the entire grid.

In the relation to the real world target space this does not seem to be so bad; first, we are only considering gasfired and turbojets for the sake of calculating performance ratios; second, it seems consistent as we assume that gasfired are cheaper, more efficient and have larger producing capacity than turbojets (although dirtier).

Anywway, there are other details that have been either abstracted in the model or maybe even in the requirements itself, for example costs not scaling with production level, kerosene pollution and having a solution that trusts at max theoretical capacities, without margins of error.

One final word: we had added a feasibility method at the beginning of the process in order to determine if the sum of all powerplants operating at their maximum capacity would be able to supply the existing demand. At the end this method is not part of the submitted solution. In this first version we just assume that the request is feasible in this way.

So this file tries to give a glimpse on the thinking process behind the developement of the solution.
