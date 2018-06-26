# DotNet-Multithreading-ScratchPad
A repo containing some twists on well known concurrency\synchronization problems
This is a repo where I will keep adding implementations of interesting variationss of multithreading problems using the C# Threading Library.
Currently it contains the following:-

1) Threading1: A Producer-consumer scenario with multiple producers & multiple consumers: 2 producers use bi-directional signalling to combine their individual contributions into a finalized product.A set of consumer threads wait to consume the output from the producers. Each output from the producers is consumed only once. 
2) Threading2: An example of the Reader-Writer with multiple readers & multiple writers scenario using reader-writer locks. Multiple writers are continually appending Key-Value pairs to a file.
Multiple reader threads are simultaneously scanning the file. Each reader is tasked with searching for a specific key & at the end must return
the latest value associated with the key. Inspired by the sstable data structure used in modern, high scale NoSQL Databases.
     Note- that there seems to be a glitch in the reader sections. the values returned are not always the latest.Need to investigate 
3) Threading3: Solves the dining philosophers problem with a set of monitors (one for each chopstick)
