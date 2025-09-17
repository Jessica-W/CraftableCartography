FROM vsmoddev AS build
WORKDIR /tmp/app
COPY CraftableCartography/CraftableCartography.csproj CraftableCartography/
COPY CakeBuild/CakeBuild.csproj CakeBuild/
ENV VINTAGE_STORY=/home/moddev/libs/
RUN dotnet restore CakeBuild
COPY . .
RUN dotnet run --project CakeBuild/CakeBuild.csproj

FROM scratch AS output
COPY --from=build /tmp/app/Releases/*.zip .