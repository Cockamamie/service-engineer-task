#!/bin/bash
ver=$(cat $1)
ver=$(($ver + 1))
echo -n "$ver" > $1