import PlatformerScene from "@/platformer-scene"

export default function Home() {
  return (
    <main className="flex min-h-screen flex-col items-center justify-center p-4 md:p-8 bg-gray-50">
      <h1 className="text-3xl md:text-4xl font-bold mb-6 text-center">2D Platformer Game Visualization</h1>
      <p className="text-gray-600 max-w-2xl text-center mb-8">
        A visual representation of a classic 2D platformer game showing the player, platforms, collectibles, enemies,
        and environment elements.
      </p>
      <PlatformerScene />
    </main>
  )
}
