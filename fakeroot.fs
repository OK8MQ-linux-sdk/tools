set -e

rawsize=8192
fatsize=65536
ext4size=4063232

totalsize=`expr $rawsize + $fatsize + $ext4size + $rawsize`

if [ -e $SDK_PATH/images/rootfs.ext4 ];then
	rm $SDK_PATH/images/rootfs.ext4
fi

if [ -e $SDK_PATH/images/rootfs.sdcard_1 ];then
	rm $SDK_PATH/images/rootfs.sdcard_1
fi

if [ -e $SDK_PATH/images/rootfs.sdcard_2 ];then
	rm $SDK_PATH/images/rootfs.sdcard_2
fi

dd if=/dev/zero of=$SDK_PATH/images/rootfs.ext4 bs=1K count=0 seek=$ext4size
echo $totalbytes
chown -h -R 0:0 $DESTDIR
# mkfs.ext4 -F -i 4096 $SDK_PATH/images/rootfs.ext4 -d $DESTDIR
chmod 4111 $DESTDIR/usr/bin/sudo
mkfs.ext4 -F -i 4096 -O ^64bit,^metadata_csum  $SDK_PATH/images/rootfs.ext4 -d $DESTDIR
fsck.ext4 -pvfD $SDK_PATH/images/rootfs.ext4

fatstart=$rawsize
fatend=`expr $rawsize + $fatsize`
ext4start=$fatend
ext4end=`expr $fatend + $ext4size`
echo $ext4end
dd if=/dev/zero of=$SDK_PATH/images/rootfs.sdcard bs=1K count=0 seek=$totalsize

parted -s $SDK_PATH/images/rootfs.sdcard mklabel msdos
parted -s $SDK_PATH/images/rootfs.sdcard unit KiB mkpart primary fat32 $fatstart $fatend
parted -s $SDK_PATH/images/rootfs.sdcard unit KiB mkpart primary $ext4start $ext4end
parted $SDK_PATH/images/rootfs.sdcard print

dd if=$SDK_PATH/images/flash_sd_emmc.bin of=$SDK_PATH/images/rootfs.sdcard conv=notrunc seek=33 bs=1K
echo $fatstartbytes
echo $ext4startbytes
dd if=$SDK_PATH/images/boot.img of=$SDK_PATH/images/rootfs.sdcard conv=notrunc,fsync seek=1K bs=$fatstart
dd if=$SDK_PATH/images/rootfs.ext4 of=$SDK_PATH/images/rootfs.sdcard conv=notrunc,fsync seek=1K bs=$ext4start

split -b 3700M $SDK_PATH/images/rootfs.sdcard $SDK_PATH/images/rootfs.sdcard
mv $SDK_PATH/images/rootfs.sdcardaa $SDK_PATH/images/rootfs.sdcard_1
mv $SDK_PATH/images/rootfs.sdcardab $SDK_PATH/images/rootfs.sdcard_2
