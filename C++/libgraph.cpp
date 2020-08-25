#include "libgraph.h"

#include <algorithm>
#include <functional>
#include <iostream>
#include <map>
#include <queue>
#include <vector>

template <typename Mapping_Type>
void mapping_insert(std::map<Mapping_Type, int32_t> &mp, Mapping_Type val) {
    if (mp.count(val))
        return;
    int32_t dfn = mp.size();
    mp[val] = dfn;
}

void parpareMapping(std::vector<ticket> const &dataSet,
                    std::map<std::string, int32_t> &mp_station,
                    std::map<int32_t, int32_t> &mp_ticketId) {
    for (auto const &t : dataSet) {
        mapping_insert(mp_station, (std::string) t.startStation);
        mapping_insert(mp_station, (std::string) t.endStation);
        mapping_insert(mp_ticketId, t.ticketId);
    }
}

std::vector<std::vector<int32_t>> genGraph(std::vector<ticket> const &dataSet,
                                           std::map<std::string, int32_t> &mp_station,
                                           std::map<int32_t, int32_t> &mp_ticketId) {
    int n = dataSet.size();
    auto ret = std::vector<std::vector<int32_t>>(n, std::vector<int32_t>());
    for (int i = 0; i < n; ++i)
        for (int j = 0; j < n; ++j)
            if ((std::string) dataSet[i].endStation == (std::string) dataSet[j].startStation && dataSet[i].arrivalTime + StayTime <= dataSet[j].startTime)
                ret[i].emplace_back(j);
    return ret;
}

std::vector<int32_t> solve(std::vector<ticket> const &dataSet,
                           std::vector<std::vector<int32_t>> const &G,
                           std::function<float(float, ticket)> calc,
                           std::string startStation,
                           std::string endStation) {
    auto que = std::queue<int>();
    auto inq = std::vector<char>(dataSet.size(), 0);
    //DP array
    auto f = std::vector<float>(dataSet.size());
    for (int i = 0; i < dataSet.size(); ++i)
        if ((std::string) dataSet[i].startStation == startStation) {
            f[i] = calc(0, dataSet[i]);
            que.push(i);
            inq[i] = 1;
        } else
            f[i] = std::numeric_limits<float>::max();

    while (! que.empty()) {
        int now = que.front();
        que.pop();

        for (auto y : G[now]) {
            float t = calc(f[now], dataSet[y]);
            if (t < f[y]) {
                f[y] = t;
                if (! inq[y])
                    que.push(y);
            }
        }

        inq[now] = 0;
    }

    float mx = std::numeric_limits<float>::max();
    int idx {};
    for (int i = 0; i < dataSet.size(); ++i)
        if ((std::string) dataSet[i].endStation == endStation) {
            if (f[i] < mx) {
                mx = f[i];
                idx = i;
            }
        }

    if (mx == std::numeric_limits<float>::max())
        return {};

    //拿到反图
    auto G2 = std::vector<std::vector<int>>(dataSet.size(), std::vector<int>());
    for (int i = 0; i < G.size(); ++i)
        for (auto y : G[i])
            G2[y].emplace_back(i);

    std::vector<int32_t> ans = { idx };
    while (dataSet[idx].startStation != startStation) {
        for (auto y : G2[idx])
            if (calc(f[y], dataSet[idx]) == f[idx]) {
                idx = y;
                break;
            }
        ans.emplace_back(idx);
    }

    std::reverse(ans.begin(), ans.end());

    return ans;
}

EXPORT std::vector<std::vector<int32_t>> find_path(std::vector<ticket> dataSet, std::string startStation, std::string endStation) {
    if (startStation == endStation) {
        perror("请站内走路到达");
        return {};
    }

    auto mapping_station = std::map<std::string, int32_t>();
    auto mapping_ticketId = std::map<int32_t, int32_t>();
    parpareMapping(std::cref(dataSet), std::ref(mapping_station), std::ref(mapping_ticketId));

    if (! mapping_station.count(startStation) || ! mapping_station.count(endStation)) {
        perror("到不了的，死心吧");
        return {};
    }

    auto G = genGraph(std::cref(dataSet), std::ref(mapping_station), std::ref(mapping_ticketId));

    std::vector<std::vector<int32_t>> ret = {};

    // 时间最短
    ret.push_back(solve(
        dataSet, G, [](float t1, ticket t2) -> float {
          return t2.arrivalTime;
        },
        startStation, endStation));

    //钱最少
    ret.push_back(solve(
        dataSet, G, [](float t1, ticket t2) -> float {
          return t1 + t2.price;
        },
        startStation, endStation));

    return ret;
}