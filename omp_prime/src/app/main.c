#include <omp.h>
#include <stdio.h>
#include <stdlib.h>
#include <stopper.h>
#include <time.h>
#include <math.h>

int main (int argc, char *argv[]) 
{
if( argc != 2 ) {
	printf("1 parameter needed: (number of threads)\n");
	return;
	}

stopperOMP st;
startSOMP(&st);

const int inputsize = 100000000;

int *numbers = malloc(sizeof *numbers * inputsize);
int tid, nthreads, i, j, ceilOfNumber, primes=0; 

nthreads = atoi(argv[1]);

omp_set_num_threads(nthreads);


//init
srand((unsigned int)time(NULL));
for (i=0; i<inputsize; i++){
	numbers[i] = rand() % 10;
}


//testing primes - dynamic schedule should be faster here, but for direct static-ish comparison we go with static 
#pragma omp parallel for schedule(static) shared(numbers) private(i,j,ceilOfNumber) reduction(+:primes)
for (i=0; i<inputsize; i++){ 
	ceilOfNumber = ceil(sqrt(numbers[i]));
	for(j=2; j<=ceilOfNumber; j++){
		if ((numbers[i]%j)==0) break;
		if (ceilOfNumber == j) {
			primes++;			
		}
	}
}	


printf("\nPrimes in the sample: %d \n", primes);

//stopper stop
stopSOMP(&st);
free(numbers);
tprintfOMP(&st, "\n");

}
