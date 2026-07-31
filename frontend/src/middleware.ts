import { NextResponse, type NextRequest } from "next/server";

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;
  const access = request.cookies.get("access_token")?.value;
  const saAccess = request.cookies.get("sa_access_token")?.value;

  const isAuthPage = pathname === "/login" || pathname === "/register";
  const isLawyerArea = pathname.startsWith("/lawyer") || pathname.startsWith("/admin");
  const isClientArea = pathname.startsWith("/client");
  const isSuperAdminArea = pathname.startsWith("/super-admin");
  const isPublic = pathname === "/" || pathname === "/health" || pathname.startsWith("/_next") || pathname.startsWith("/favicon");

  if (isPublic) {
    if (pathname === "/") {
      if (saAccess) return NextResponse.redirect(new URL("/super-admin/dashboard", request.url));
      if (access) return NextResponse.redirect(new URL("/lawyer", request.url));
      return NextResponse.redirect(new URL("/login", request.url));
    }
    return NextResponse.next();
  }

  // Super admin area (except its login page which is public)
  if (isSuperAdminArea) {
    const isSALogin = pathname === "/super-admin/login";
    if (isSALogin) {
      if (saAccess) return NextResponse.redirect(new URL("/super-admin/dashboard", request.url));
      return NextResponse.next();
    }
    if (!saAccess) return NextResponse.redirect(new URL("/super-admin/login", request.url));
    return NextResponse.next();
  }

  // Auth pages
  if (isAuthPage) {
    if (access) return NextResponse.redirect(new URL("/lawyer", request.url));
    return NextResponse.next();
  }

  // Protected areas
  if (isLawyerArea || isClientArea) {
    if (!access) {
      const url = new URL("/login", request.url);
      url.searchParams.set("next", pathname);
      return NextResponse.redirect(url);
    }
    return NextResponse.next();
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/((?!api|_next/static|_next/image|.*\\.(?:png|jpg|jpeg|svg|gif|ico|woff2?)$).*)"]
};
