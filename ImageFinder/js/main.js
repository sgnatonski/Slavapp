var vptree = undefined;

function build(hashes) {
    vptree = VPTreeFactory.build(hashes, hamming_distance);
}

function search(hash, n, maxDistance) {
    var nearest = vptree.search(hash, n, maxDistance);
    return nearest;
}