"use client"

import { useEffect, useRef, useState } from "react"

export default function PlatformerScene() {
  const canvasRef = useRef<HTMLCanvasElement>(null)
  const [dimensions, setDimensions] = useState({ width: 800, height: 450 })

  useEffect(() => {
    const handleResize = () => {
      const container = document.getElementById("canvas-container")
      if (container) {
        const width = container.clientWidth
        const height = Math.min(450, Math.floor(width * 0.5625)) // 16:9 aspect ratio with max height
        setDimensions({ width, height })
      }
    }

    handleResize()
    window.addEventListener("resize", handleResize)
    return () => window.removeEventListener("resize", handleResize)
  }, [])

  useEffect(() => {
    if (!canvasRef.current) return

    const canvas = canvasRef.current
    const ctx = canvas.getContext("2d")
    if (!ctx) return

    // Set canvas dimensions
    canvas.width = dimensions.width
    canvas.height = dimensions.height

    // Scale factors based on canvas size
    const scaleX = dimensions.width / 800
    const scaleY = dimensions.height / 450

    // Draw background
    const gradient = ctx.createLinearGradient(0, 0, 0, canvas.height)
    gradient.addColorStop(0, "#87CEEB") // Sky blue
    gradient.addColorStop(1, "#E0F7FF") // Light blue
    ctx.fillStyle = gradient
    ctx.fillRect(0, 0, canvas.width, canvas.height)

    // Draw clouds
    ctx.fillStyle = "rgba(255, 255, 255, 0.8)"
    drawCloud(ctx, 100 * scaleX, 80 * scaleY, 60 * scaleX, 30 * scaleY)
    drawCloud(ctx, 300 * scaleX, 60 * scaleY, 80 * scaleX, 40 * scaleY)
    drawCloud(ctx, 600 * scaleX, 100 * scaleY, 70 * scaleX, 35 * scaleY)

    // Draw mountains in background
    ctx.fillStyle = "#9CB4CC"
    drawMountain(ctx, 150 * scaleX, 200 * scaleY, 200 * scaleX, 150 * scaleY)
    drawMountain(ctx, 400 * scaleX, 180 * scaleY, 250 * scaleX, 170 * scaleY)
    drawMountain(ctx, 650 * scaleX, 220 * scaleY, 180 * scaleX, 130 * scaleY)

    // Draw ground
    ctx.fillStyle = "#8B4513" // Brown
    ctx.fillRect(0, 400 * scaleY, canvas.width, 50 * scaleY)

    // Draw grass on top of ground
    ctx.fillStyle = "#228B22" // Forest green
    ctx.fillRect(0, 400 * scaleY, canvas.width, 10 * scaleY)

    // Draw platforms
    drawPlatform(ctx, 100 * scaleX, 350 * scaleY, 200 * scaleX, 15 * scaleY)
    drawPlatform(ctx, 400 * scaleX, 300 * scaleY, 150 * scaleX, 15 * scaleY)
    drawPlatform(ctx, 600 * scaleX, 250 * scaleY, 180 * scaleX, 15 * scaleY)
    drawPlatform(ctx, 250 * scaleX, 200 * scaleY, 120 * scaleX, 15 * scaleY)

    // Draw collectible coins
    ctx.fillStyle = "#FFD700" // Gold
    drawCoin(ctx, 150 * scaleX, 320 * scaleY, 10 * scaleX)
    drawCoin(ctx, 200 * scaleX, 320 * scaleY, 10 * scaleX)
    drawCoin(ctx, 250 * scaleX, 320 * scaleY, 10 * scaleX)
    drawCoin(ctx, 450 * scaleX, 270 * scaleY, 10 * scaleX)
    drawCoin(ctx, 500 * scaleX, 270 * scaleY, 10 * scaleX)
    drawCoin(ctx, 650 * scaleX, 220 * scaleY, 10 * scaleX)
    drawCoin(ctx, 700 * scaleX, 220 * scaleY, 10 * scaleX)
    drawCoin(ctx, 300 * scaleX, 170 * scaleY, 10 * scaleX)

    // Draw enemies
    drawEnemy(ctx, 350 * scaleX, 385 * scaleY, 20 * scaleX, 15 * scaleY)
    drawEnemy(ctx, 500 * scaleX, 385 * scaleY, 20 * scaleX, 15 * scaleY)
    drawEnemy(ctx, 450 * scaleX, 285 * scaleY, 20 * scaleX, 15 * scaleY)

    // Draw player character
    drawPlayer(ctx, 50 * scaleX, 370 * scaleY, 30 * scaleX, 30 * scaleY)

    // Draw game elements
    drawGameHUD(ctx, canvas.width, canvas.height, scaleX, scaleY)
  }, [dimensions])

  // Helper functions for drawing
  function drawCloud(ctx: CanvasRenderingContext2D, x: number, y: number, width: number, height: number) {
    ctx.beginPath()
    ctx.arc(x, y, height, 0, Math.PI * 2)
    ctx.arc(x + width * 0.3, y - height * 0.2, height * 0.8, 0, Math.PI * 2)
    ctx.arc(x + width * 0.6, y, height * 0.9, 0, Math.PI * 2)
    ctx.fill()
  }

  function drawMountain(ctx: CanvasRenderingContext2D, x: number, y: number, width: number, height: number) {
    ctx.beginPath()
    ctx.moveTo(x - width / 2, y)
    ctx.lineTo(x, y - height)
    ctx.lineTo(x + width / 2, y)
    ctx.closePath()
    ctx.fill()
  }

  function drawPlatform(ctx: CanvasRenderingContext2D, x: number, y: number, width: number, height: number) {
    // Platform base
    ctx.fillStyle = "#8B4513" // Brown
    ctx.fillRect(x, y, width, height)

    // Grass on top
    ctx.fillStyle = "#228B22" // Forest green
    ctx.fillRect(x, y, width, 5)
  }

  function drawCoin(ctx: CanvasRenderingContext2D, x: number, y: number, radius: number) {
    ctx.beginPath()
    ctx.arc(x, y, radius, 0, Math.PI * 2)
    ctx.fill()

    // Inner highlight
    ctx.fillStyle = "#FFF8DC" // Cornsilk
    ctx.beginPath()
    ctx.arc(x, y, radius * 0.7, 0, Math.PI * 2)
    ctx.fill()
  }

  function drawEnemy(ctx: CanvasRenderingContext2D, x: number, y: number, width: number, height: number) {
    // Enemy body
    ctx.fillStyle = "#FF4500" // OrangeRed
    ctx.fillRect(x - width / 2, y - height, width, height)

    // Enemy eyes
    ctx.fillStyle = "white"
    ctx.beginPath()
    ctx.arc(x - width / 4, y - height * 0.7, width * 0.15, 0, Math.PI * 2)
    ctx.arc(x + width / 4, y - height * 0.7, width * 0.15, 0, Math.PI * 2)
    ctx.fill()

    // Enemy pupils
    ctx.fillStyle = "black"
    ctx.beginPath()
    ctx.arc(x - width / 4, y - height * 0.7, width * 0.05, 0, Math.PI * 2)
    ctx.arc(x + width / 4, y - height * 0.7, width * 0.05, 0, Math.PI * 2)
    ctx.fill()
  }

  function drawPlayer(ctx: CanvasRenderingContext2D, x: number, y: number, width: number, height: number) {
    // Player body
    ctx.fillStyle = "#4169E1" // RoyalBlue
    ctx.fillRect(x, y - height, width, height)

    // Player head
    ctx.fillStyle = "#FFA07A" // LightSalmon
    ctx.beginPath()
    ctx.arc(x + width / 2, y - height - width * 0.4, width * 0.4, 0, Math.PI * 2)
    ctx.fill()

    // Player eyes
    ctx.fillStyle = "white"
    ctx.beginPath()
    ctx.arc(x + width * 0.35, y - height - width * 0.4, width * 0.1, 0, Math.PI * 2)
    ctx.arc(x + width * 0.65, y - height - width * 0.4, width * 0.1, 0, Math.PI * 2)
    ctx.fill()

    // Player pupils
    ctx.fillStyle = "black"
    ctx.beginPath()
    ctx.arc(x + width * 0.35, y - height - width * 0.4, width * 0.05, 0, Math.PI * 2)
    ctx.arc(x + width * 0.65, y - height - width * 0.4, width * 0.05, 0, Math.PI * 2)
    ctx.fill()
  }

  function drawGameHUD(
    ctx: CanvasRenderingContext2D,
    canvasWidth: number,
    canvasHeight: number,
    scaleX: number,
    scaleY: number,
  ) {
    // Score display
    ctx.fillStyle = "rgba(0, 0, 0, 0.5)"
    ctx.fillRect(10 * scaleX, 10 * scaleY, 150 * scaleX, 30 * scaleY)

    ctx.fillStyle = "white"
    ctx.font = `${16 * Math.min(scaleX, scaleY)}px Arial`
    ctx.fillText("SCORE: 1250", 20 * scaleX, 30 * scaleY)

    // Lives display
    ctx.fillStyle = "rgba(0, 0, 0, 0.5)"
    ctx.fillRect(canvasWidth - 110 * scaleX, 10 * scaleY, 100 * scaleX, 30 * scaleY)

    ctx.fillStyle = "white"
    ctx.fillText("LIVES: 3", canvasWidth - 100 * scaleX, 30 * scaleY)

    // Coin counter
    ctx.fillStyle = "#FFD700" // Gold
    drawCoin(ctx, canvasWidth - 140 * scaleX, 25 * scaleY, 10 * scaleX)

    ctx.fillStyle = "white"
    ctx.fillText("× 8", canvasWidth - 125 * scaleX, 30 * scaleY)
  }

  return (
    <div className="flex flex-col items-center w-full">
      <div
        id="canvas-container"
        className="w-full max-w-4xl border-4 border-gray-800 rounded-lg overflow-hidden bg-white"
      >
        <canvas ref={canvasRef} width={dimensions.width} height={dimensions.height} className="w-full" />
      </div>
      <div className="mt-6 bg-gray-100 p-4 rounded-lg max-w-4xl w-full">
        <h3 className="text-lg font-bold mb-2">Key Elements of a 2D Platformer:</h3>
        <ul className="grid grid-cols-1 md:grid-cols-2 gap-2">
          <li className="flex items-center">
            <div className="w-4 h-4 bg-blue-600 mr-2"></div>
            <span>Player Character - can run, jump, and interact with the environment</span>
          </li>
          <li className="flex items-center">
            <div className="w-4 h-4 bg-green-700 mr-2"></div>
            <span>Platforms - for the player to jump on and traverse</span>
          </li>
          <li className="flex items-center">
            <div className="w-4 h-4 bg-yellow-400 mr-2"></div>
            <span>Collectibles - coins, power-ups, and other items to gather</span>
          </li>
          <li className="flex items-center">
            <div className="w-4 h-4 bg-red-500 mr-2"></div>
            <span>Enemies - obstacles that can damage the player</span>
          </li>
          <li className="flex items-center">
            <div className="w-4 h-4 bg-gray-400 mr-2"></div>
            <span>Environment - background elements that create atmosphere</span>
          </li>
          <li className="flex items-center">
            <div className="w-4 h-4 bg-black mr-2"></div>
            <span>HUD - displays score, lives, and other game information</span>
          </li>
        </ul>
      </div>
    </div>
  )
}
