# Maintainer: Maurice Panchaud <github at mauricepanchaud dot com>
pkgname=switchtube-dl-bin
pkgver=1.0.0
pkgrel=1 # Increment when PKGBUILD is changed
pkgdesc="Simple CLI for downloading videos from SwitchTube"
arch=('x86_64')
url="https://github.com/panmau/switchtube-dl"
license=('MIT')
depends=()
makedepends=()
provides=('switchtube-dl')
source=("$pkgname-$pkgver.tar.gz::https://github.com/panmau/switchtube-dl/releases/download/$pkgver/switchtube-dl.${pkgver}.linux-x64.tar.gz")
sha256sums=('SKIP')

package() {
	cd "$pkgname-$pkgver"
    install -Dm755 -t "${pkgdir}/usr/bin" \
        ./clockify-cli
    install -Dm644 ./LICENSE \
        "${pkgdir}/usr/share/licenses/${pkgname}/LICENSE"
    install -Dm644 ./README.md \
        "${pkgdir}/usr/share/doc/${pkgname}/README.md"
}
