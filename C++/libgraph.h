#ifndef TEST_CLION_LIBGRAPH_H
#define TEST_CLION_LIBGRAPH_H

#ifdef BUILD_DLL
#define EXPORT __declspec(dllexport)
#else
#define EXPORT __declspec(dllimport)
#endif

#include <cstdint>
#include <vector>
#include <string>

int const StayTime = 0; //换乘时间

struct EXPORT ticket {
    int32_t ticketId;
    char startStation[30];
    int32_t startTime;
    char endStation[30];
    int32_t arrivalTime;
    char trainId[10];
    float price;
};

EXPORT std::vector<std::vector<int32_t>> find_path(std::vector<ticket> dataSet, std::string startStation, std::string endStation);

#endif //TEST_CLION_LIBGRAPH_H
