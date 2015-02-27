/* 
 * File:   main.cpp
 * Author: root
 *
 * Created on December 3, 2014, 1:31 PM
 */

#include <cstdlib>
#include <unistd.h>
#include <stdio.h>
#include <list>

#include <Utilities/Thread.h>
#include <Utilities/String.h>

using namespace std;

void* worker(void* name)
{
    String lName = *(String*)name;
    for(int i = 0; i < 100; i++)
    {
        printf("%s: %d\n", lName.c_str(), i);
        sleep(2);
    }
    return NULL;
}

/*
 * 
 */
int main(int argc, char** argv)
{
    char nam[255];
    std::list<utilities::Thread*> threads;
    for(int i = 0; i < 100; i++)
    {
        sprintf(nam, "th_%d", i);
        String name = utilities::newString(nam);
        threads.push_back(new utilities::Thread(&(worker), &name));
        usleep(120);
    }
    bool alive = true;
    while(alive)
    {
        alive = false;
        std::list<utilities::Thread*>::const_iterator itr = threads.begin();
        while(itr != threads.end())
        {
            if((*itr)->isAlive())
                alive = true;
            itr++;
        }
    }
    char a[255];
    printf("Finished.\n");
    scanf(a);
    return 0;
}

