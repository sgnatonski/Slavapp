/*

pHash, the open source perceptual hash library
Copyright (C) 2009 Aetilius, Inc.
All rights reserved.

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

Evan Klinger - eklinger@phash.org
D Grant Starkweather - dstarkweather@phash.org

*/

#include "pHash.h"
#ifndef _WIN32
#include "config.h"
#else
#define snprintf _snprintf
#endif

CImg<float>* ph_dct_matrix(const int N){
    CImg<float> *ptr_matrix = new CImg<float>(N,N,1,1,1/sqrt((float)N));
    const float c1 = sqrt(2.0f/N); 
    for (int x=0;x<N;x++){
        for (int y=1;y<N;y++){
            *ptr_matrix->data(x,y) = c1*(float)cos((cimg::PI/2/N)*y*(2*x+1));
        }
    }
    return ptr_matrix;
}

int ph_dct_imagehash(const char* file,ulong64 &hash){

    if (!file){
        return -1;
    }
    CImg<uint8_t> src;
    try {
        src.load(file);
    } catch (CImgIOException ex){
        return -1;
    }
    CImg<float> meanfilter(7,7,1,1,1);
    CImg<float> img;
    if (src.spectrum() == 3){
        img = src.RGBtoYCbCr().channel(0).get_convolve(meanfilter);
    } else if (src.spectrum() == 4){
        int width = img.width();
        int height = img.height();
        int depth = img.depth();
        img = src.crop(0,0,0,0,width-1,height-1,depth-1,2).RGBtoYCbCr().channel(0).get_convolve(meanfilter);
    } else {
        img = src.channel(0).get_convolve(meanfilter);
    }

    img.resize(32,32);
    CImg<float> *C  = ph_dct_matrix(32);
    CImg<float> Ctransp = C->get_transpose();

    CImg<float> dctImage = (*C)*img*Ctransp;

    CImg<float> subsec = dctImage.crop(1,1,8,8).unroll('x');;

    float median = subsec.median();
    ulong64 one = 0x0000000000000001;
    hash = 0x0000000000000000;
    for (int i=0;i< 64;i++){
        float current = subsec(i);
        if (current > median)
            hash |= one;
        one = one << 1;
    }

    delete C;

    return 0;
}

int ph_dct_imagehash2(uint8_t *pxarray, int width, int height, ulong64 &hash) {

	if (!pxarray) {
		return -1;
	}
	CImg<uint8_t> src(pxarray, width, height);
	CImg<float> meanfilter(7, 7, 1, 1, 1);
	CImg<float> img;
	if (src.spectrum() == 3) {
		img = src.RGBtoYCbCr().channel(0).get_convolve(meanfilter);
	}
	else if (src.spectrum() == 4) {
		int width = img.width();
		int height = img.height();
		int depth = img.depth();
		img = src.crop(0, 0, 0, 0, width - 1, height - 1, depth - 1, 2).RGBtoYCbCr().channel(0).get_convolve(meanfilter);
	}
	else {
		img = src.channel(0).get_convolve(meanfilter);
	}

	img.resize(32, 32);
	CImg<float> *C = ph_dct_matrix(32);
	CImg<float> Ctransp = C->get_transpose();

	CImg<float> dctImage = (*C)*img*Ctransp;

	CImg<float> subsec = dctImage.crop(1, 1, 8, 8).unroll('x');;

	float median = subsec.median();
	ulong64 one = 0x0000000000000001;
	hash = 0x0000000000000000;
	for (int i = 0; i< 64; i++) {
		float current = subsec(i);
		if (current > median)
			hash |= one;
		one = one << 1;
	}

	delete C;

	return 0;
}
